using Application.Constants;
using Application.Contracts;
using Application.Models;
using Core.Configuration;
using Core.Contracts;
using Core.Extensions;
using Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Services;

/// <summary>
/// Provides services for retrieving and processing job listings from Google,  and updating company profiles and job
/// data in the repository.
/// </summary>
/// <remarks>This service integrates with various components to fetch job listings,  infer additional information
/// using AI, and store the results. It handles  both job data and company profiles, ensuring that the information is 
/// up-to-date and accurately reflects the current job market as retrieved  from Google.</remarks>
public class SearpApiSourceService : IJobSourceService
{
    private static readonly List<string> _tokens =
    [
        "lead",
        "manager",
        "director",
        "ceo",
        "president",
        "data",
        "engineer",
    ];
    private static readonly string[] _validExtensions = [".com", ".org", ".net", ".io", ".ai", ".dev", "en", "/"];
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;
    private readonly ISearch<OrganicResult> _searchService;
    private readonly IJobsRetrieval<JobResult> _jobsRetrieval;
    private readonly IOpenAiChatService _aiChatService;
    private readonly ICompanyRepository _companyRepository;
    private readonly IJobsRepository _jobsRepository;
    private readonly SerpApiSettings _settings;
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new ClientResultJsonConverter<ChatCompletion>() }
    };

    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public string Id { get; } = nameof(SearpApiSourceService).GenHashString();

    /// <summary>
    /// Gets or sets the description of the job source service.
    /// </summary>
    public string Description { get; set; } = "Google Job Source Service";

    /// <summary>
    /// Initializes a new instance of the <see cref="SearpApiSourceService"/> class.
    /// </summary>
    /// <param name="options">The configuration settings for the Serp API.</param>
    /// <param name="cacheService">The cache service used for caching search results.</param>
    /// <param name="searchService">The search service used for querying job results.</param>
    /// <param name="aiChatService">The AI chat service used for processing job-related queries.</param>
    /// <param name="jobsRetrieval">The service responsible for retrieving job listings.</param>
    /// <param name="jobsRepository">The repository for storing and managing job data.</param>
    /// <param name="companyRepository">The repository for storing and managing company data.</param>
    /// <param name="logger">The logger used for logging information and errors.</param>
    public SearpApiSourceService(
        IOptions<SerpApiSettings> options,
        ICacheService cacheService,
        ISearch<OrganicResult> searchService,
        IOpenAiChatService aiChatService,
        IJobsRetrieval<JobResult> jobsRetrieval,
        IJobsRepository jobsRepository,
        ICompanyRepository companyRepository,
        ILogger<SearpApiSourceService> logger)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(jobsRetrieval, nameof(jobsRetrieval));
        ArgumentNullException.ThrowIfNull(jobsRepository, nameof(jobsRepository));
        ArgumentNullException.ThrowIfNull(companyRepository, nameof(companyRepository));
        ArgumentNullException.ThrowIfNull(aiChatService, nameof(aiChatService));
        ArgumentNullException.ThrowIfNull(searchService, nameof(searchService));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _logger = logger;
        _searchService = searchService;
        _settings = options.Value;
        _aiChatService = aiChatService;
        _jobsRetrieval = jobsRetrieval;
        _jobsRepository = jobsRepository;
        _companyRepository = companyRepository;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Updates or adds company profiles based on recent job data.
    /// </summary>
    /// <remarks>This method retrieves jobs from the past 30 days and updates the company profiles
    /// accordingly. If a company profile does not exist for a job, it attempts to create one using AI-generated
    /// hierarchy results. Logs any exceptions encountered during the update process.</remarks>
    /// <returns><see langword="true"/> if the company profiles were successfully updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> UpdateCompanyProfilesAsync()
    {
        var timeStamp = DateTime.Now.AddDays(-30);
        var jobs = await _jobsRepository.GetJobsAsync(timeStamp);
        if (jobs is null || !jobs.Any())
        {
            return false;
        }

        foreach (var job in jobs)
        {
            try
            {
                string sanitizedJson = string.Empty;
                _logger.LogInformation("Processing hierarchy response for company: {CompanyName}", job.CompanyName);

                var companyProfile = await _companyRepository.GetCompanyProfileAsync(job.CompanyId);
                if (companyProfile is not null && companyProfile.UpdatedAt >= DateTime.Now.AddDays(-1))
                {
                    _logger.LogInformation("Company profile does not need updating: {CompanyName}", job.CompanyName);
                    continue;
                }

                var prompt = $"{job.CompanyName} official site";
                var googleResults = await _searchService.FetchOrganicResults(prompt, _settings.Location);
                if (googleResults is null || !googleResults.Any())
                {
                    _logger.LogWarning("No Google results found for company: {CompanyName}", job.CompanyName);
                    continue;
                }

                var snippets = googleResults
                                   .Select(result => result.Snippet)
                                   .ToList();

                string snippet = string.Join(" ", snippets.Select(link => $"site:{link}"));
                string domainname = CoreRegex.ExtractDomainName(snippet);
                if (!string.IsNullOrWhiteSpace(domainname))
                {
                    prompt = $"{job.CompanyName.Replace(" ", string.Empty).ToLower()} organizational structure {job.Division} leadership team";
                }
                else
                {
                    prompt = $"{job.CompanyName.Replace(" ", string.Empty).ToLower()} organizational structure leadership team";
                }

                googleResults = await _searchService.FetchOrganicResults(prompt, _settings.Location);
                if (googleResults is null || !googleResults.Any())
                {
                    _logger.LogWarning("No Google results found for company: {CompanyName}", job.CompanyName);
                    continue;
                }
                var organicResults = string.Join("\r\n", googleResults.Select(r => r.Snippet));
                if (!_tokens.Any(s => organicResults.Contains(s, StringComparison.CurrentCultureIgnoreCase)))
                {
                    _logger.LogWarning("No titles found for company: {CompanyName}", job.CompanyName);
                    continue;
                }

                var clientResult = await GetSearchResults(job, organicResults);
                if (clientResult is not null)
                {
                    try
                    {
                        if (companyProfile is not null)
                        {
                          await _companyRepository.UpdateCompanyProfileAsync(new CompanyProfile(job, clientResult));
                            _logger.LogInformation("Updated existing company profile for: {CompanyName}", job.CompanyName);
                        }
                        else
                        {
                            await _companyRepository.AddCompanyProfileAsync(new CompanyProfile(job, clientResult));
                            _logger.LogInformation("Added new company profile for: {CompanyName}", job.CompanyName);
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        // Log the JSON parsing error
                        _logger.LogError("JSON parsing error for company: {CompanyName}. Error: {Message} \r\n Json: {sanitizedJson}", job.CompanyName, jsonEx.Message, sanitizedJson);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here, but should be done in a real application)
                _logger.LogError("Error updating company profiles: {Message}", ex.Message);
            }
        }

        var results = await _companyRepository.GetCompanyProfileAsync(timeStamp);
        return jobs.Count() == results.Count();
    }

    private async Task<HierarchyResults?> GetSearchResults(JobSummary job, string organicResults)
    {
        var cacheKey = CachingHelper.GenCacheKey(nameof(SearpApiSourceService), job.CompanyName, organicResults.GenHashString());
        var cachedSearch = await _cacheService.TryGetAsync<HierarchyResults>(cacheKey);
        if (cachedSearch is not null)
        {
            _logger.LogInformation("Returning cached OpenAi result for key: {CacheKey}", cacheKey);
            return cachedSearch;
        }

        var description = job.JobDescription[..Math.Min(job.JobDescription.Length, 1500)];
        var results = organicResults[..Math.Min(organicResults.Length, 1500)];
        List<ChatMessage> messages =
        [
            new SystemChatMessage("You are an expert in organizational analysis and job market trends."),
            new UserChatMessage($"You are analyzing a job posting and a company's public org chart.\r\n\r\nYour goal is to identify the most relevant reporting chain or leadership team responsible for hiring this role — starting from the closest manager up to the relevant VP, but excluding unrelated executives.\r\n\r\nUse the job description and org structure data below.\r\n\r\nCompany: {job.CompanyName}\r\n\r\nJob Description:\r\n{description}\r\n\r\nOrganizational Structure:\r\n{results}\r\n\r\nReturn the result in this format:\r\n\r\n{{\r\n  \"orghierarchy\": [\r\n    {{ \"name\": \"Closest Likely Hiring Manager\", \"title\": \"Relevant Manager Title\" }},\r\n    {{ \"name\": \"Their Director\", \"title\": \"Director Title\" }},\r\n    {{ \"name\": \"Relevant VP or Practice Lead\", \"title\": \"VP Title\" }}\r\n  ]\r\n}}\r\n\r\nOnly include names and job titles that match the functional domain described in the job. Do not include global executives unless they directly oversee the hiring scope. If titles include unrelated roles (e.g. CFO, CMO), omit them.\r\n")
        ];

        string sanitizedJson = string.Empty;
        try
        {
            var chatClient = _aiChatService.Client.GetChatClient(_aiChatService.Configuration.Model);
            var response = await chatClient.CompleteChatAsync(messages);
            if (response is not null && response.Value is not null && response.Value.Content.Any())
            {
                var text = response.Value.Content?.FirstOrDefault()?.Text ?? null;
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("Empty organizational hierarchy response for company: {CompanyName}", job.CompanyName);
                    return null;
                }

                var sanitized = CoreRegex.SanitizeJson(text);
                sanitizedJson = JsonHelpers.ExtractJson(sanitized);
                var result = JsonSerializer.Deserialize<HierarchyResults>(sanitizedJson, _options);
                await _cacheService.CreateEntryAsync(cacheKey, result);
                return result;
            }
            _logger.LogWarning("No content returned from AI chat completion for company: {CompanyName}", job.CompanyName);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving cached search results: {Message} \r\nFailing json: {sanitizedJson}", ex.Message, sanitizedJson);
        }
        return null;
    }

    /// <summary>
    /// Updates the job source by retrieving job data and processing it for storage.
    /// </summary>
    /// <remarks>This method fetches job listings based on the configured query and location settings. It
    /// processes each job to infer division information using an AI chat service and stores the summarized job data in
    /// the repository. If no jobs are retrieved or an error occurs during processing, the method returns <see
    /// langword="false"/>. Otherwise, it returns <see langword="true"/> upon successful completion.</remarks>
    /// <returns><see langword="true"/> if the job source is successfully updated; otherwise,  <see langword="false"/> if no jobs
    /// are found or an error occurs.</returns>
    public async Task<bool> UpdateJobSourceAsync()
    {
        var jobs = await _jobsRetrieval.FetchJobs(_settings.Query, _settings.Location);
        if (jobs is null || !jobs.Any())
        {
            return false;
        }

        foreach (var job in jobs)
        {
            try
            {
                if (_jobsRepository.GetJobsAsync(job.JobId).Result is not null)
                {
                    _logger.LogInformation("Job with ID {JobId} already exists, skipping.", job.JobId);
                    continue;
                }

                var prompt = Prompts.DivisionMessage.Replace("{CompanyName}", job.CompanyName).Replace("{Description}", job.Description);
                var divisionResults = await _aiChatService.GetChatCompletion<DivisionInference>(Prompts.DivisionSystem, prompt);
                var summary = new JobSummary(job)
                {
                    Division = divisionResults?.Division ?? string.Empty,
                    Reasoning = divisionResults?.Reasoning ?? string.Empty,
                    Confidence = divisionResults?.Confidence ?? 0
                };
                await _jobsRepository.AddJobAsync(summary);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here, but should be done in a real application)
                _logger.LogError("Error updating job source: {Message}", ex.Message);
                return false;
            }
        }
        return true;
    }
}

/// <summary>
/// Represents the results of an organizational hierarchy query.
/// </summary>
/// <remarks>This class provides a collection of <see cref="HierarchyItem"/> that describe the structure of an
/// organization. The <see cref="OrgHierarchy"/> property contains the hierarchical data.</remarks>
public class HierarchyResults
{
    /// <summary>
    /// Gets or sets the organizational hierarchy as a list of hierarchy items.
    /// </summary>
    [JsonPropertyName("orghierarchy")]
    public List<HierarchyItem> OrgHierarchy { get; set; } = [];
}

/// <summary>
/// Represents an item in a hierarchy with a name and a title.
/// </summary>
public class HierarchyItem
{
    /// <summary>
    /// Gets or sets the name associated with the entity.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the item.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}