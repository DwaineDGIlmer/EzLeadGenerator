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
using System.Text.RegularExpressions;

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
    private static readonly List<string> _pronouns = ["I", "you", "he", "she", "it", "we", "they", "me", "him", "her", "us", "them", "their"];
    private static readonly List<string> _conjunctions = ["and", "but", "or", "yet", "for", "nor", "so", "not"];
    private static readonly List<string> _wordsNotInNames = ["lifelong", "lastname", "firstname", "executive", "role", "provided", "chief", "information", "officer", "data", "architect", "doe", "relevant", "practice", "vp", "director", "lead", "closest", "likely", "staff", "engineer", "engineering", "unknown", "n/a", "not applicable", "no data", "none", "null"];
    private static readonly List<string> _invalidNames = ["mike johnson", "john smith", "jane smith"];
    private static readonly List<string> _titleWords = ["engineer", "engineering", "aws", "level", "lead", "manager", "supervisor", "principal", "analyst", "hybrid", "remote", "analytics", "automation", "architect"];
    private static readonly List<string>_recruitingCompanies =     
    [
        "Recruit",
        "Yeah! Global",
        "Motion Recruitment",
        "Jobs via Dice",
        "Insight Global",
        "Jobright.ai",
        "CyberCoders",
        "Recruitment",
        "Talent",
        "Staffing"
    ];
    private static readonly List<string> _tokens =
    [
        "lead",
        "data",
        "engineer",
        "manager",
        "director",
        "practice",
        "Data Science",
        "Data Analytics",
        "Data Scientist",
        "Data Analyst",
        "Business Intelligence (BI)",
        "Data Strategy",
        "Data Governance",
        "Data Management",
        "Data Architecture",
        "Data Operations",
        "Data Solutions",
    ];
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;
    private readonly ISearch _searchService;
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
        ISearch searchService,
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
                if (companyProfile is not null && companyProfile.UpdatedAt >= DateTime.Now.AddDays(-3))
                {
                    _logger.LogInformation("Company profile does not need updating: {CompanyName}", job.CompanyName);
                    continue;
                }

                var prompt = $"{job.CompanyName} official site";
                var googleResults = await _searchService.FetchOrganicResults<OrganicResult>(prompt, _settings.Location);
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
                string link = googleResults.FirstOrDefault()?.Link ?? string.Empty;
                if (string.IsNullOrWhiteSpace(domainname))
                {
                    var names = job.CompanyName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in googleResults)
                    {
                        var links = new[] { item.Link };
                        foreach (var candidateLink in links)
                        {
                            if (names.Any(name => candidateLink.Contains(name, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                domainname = CoreRegex.ExtractDomainName(candidateLink);
                                link = candidateLink;
                                _logger.LogInformation("Extracted domain name: {DomainName} for company: {CompanyName}", domainname, job.CompanyName);
                                break;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(domainname))
                        {
                            break;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(job.Division))
                {
                    prompt = $"{job.CompanyName} organizational structure {job.Division} data engineering leadership team";
                }
                else
                {
                    prompt = $"{job.CompanyName} organizational structure data engineering leadership team";
                }

                googleResults = await _searchService.FetchOrganicResults<OrganicResult>(prompt, _settings.Location);
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
                            await _companyRepository.UpdateCompanyProfileAsync(new CompanyProfile(job, clientResult)
                            {
                                Link = link,
                                DomainName = domainname,
                            });
                            _logger.LogInformation("Updated existing company profile for: {CompanyName}", job.CompanyName);
                        }
                        else
                        {
                            await _companyRepository.AddCompanyProfileAsync(new CompanyProfile(job, clientResult)
                            {
                                Link = link,
                                DomainName = domainname,
                            });
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

    /// <summary>
    /// Updates the job title of the specified <see cref="JobResult"/> object based on predefined rules.
    /// </summary>
    /// <remarks>This method modifies the <paramref name="job"/> object's <c>Title</c> property to ensure it
    /// is valid and meaningful. If the title is empty, consists only of whitespace, or matches the company name, it is
    /// set to "Data Engineer". Additionally, the method trims invalid characters and adjusts the title based on
    /// predefined keywords.</remarks>
    /// <param name="job">The <see cref="JobResult"/> object whose <c>Title</c> property will be updated. Cannot be <c>null</c>.</param>
    public static void UpdateJobTitle(JobResult job)
    {
        if (string.IsNullOrEmpty(job.Title) ||
            job.Title.Equals(" ") ||
            job.Title.Equals(job.CompanyName))
        {
            job.Title = "Data Engineer";
            return;
        }
        // Preserve titles ending with " I", " II", or " III"
        if (Regex.IsMatch(job.Title, @"\b(I|II|III)$"))
        {
            return;
        }

        var match = Regex.Match(job.Title, @"[^a-zA-Z0-9 -]");
        if (match.Success && match.Index > 0)
        {
            job.Title = job.Title[..match.Index].Trim();
        }
        // Find all matches with their indexes
        var matches = _titleWords
            .Select(word => new { Word = word, Index = job.Title.IndexOf(word, StringComparison.OrdinalIgnoreCase) })
            .Where(x => x.Index != -1);

        // Get the match with the highest index (last in the string)
        var lastMatch = matches.OrderByDescending(x => x.Index).FirstOrDefault();
        if (lastMatch != null)
        {
            // Include the found word in the result
            int endIndex = Math.Min(lastMatch.Index + lastMatch.Word.Length, job.Title.Length);
            job.Title = job.Title[..endIndex];
        }
    }

    /// <summary>
    /// Updates the job source by retrieving jobs, validating their data, and adding them to the repository.
    /// </summary>
    /// <remarks>This method fetches jobs based on the configured query and location settings, validates each
    /// job's data, and adds valid jobs to the repository. Jobs are skipped if they already exist, lack required
    /// information, are remote, or are outside the specified geographic area. The method uses AI-based inference to
    /// determine the division of each job before adding it to the repository.</remarks>
    /// <returns><see langword="true"/> if the job source was successfully updated with at least one valid job; otherwise, <see
    /// langword="false"/>.</returns>
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
                if (await _jobsRepository.GetJobAsync(job.JobId) is not null)
                {
                    _logger.LogInformation("Job with ID {JobId} already exists, skipping.", job.JobId);
                    continue;
                }

                // Validate the job
                if (!IsValid(job, _logger))
                {
                    _logger.LogInformation("Job with ID {JobId} is not valid, skipping.", job.JobId);
                    continue;
                }

                UpdateJobTitle(job);

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

    /// <summary>
    /// Validates the specified job result to ensure it meets the criteria for being added to the repository.
    /// </summary>
    /// <param name="job">The job to validate.</param>
    /// <param name="logger">The Logger</param>
    /// <returns>True if valid otherwise false.</returns>
    public static bool IsValid(JobResult job, ILogger logger)
    {
        // Check the Job summary for missing information
        if (string.IsNullOrWhiteSpace(job.CompanyName) || string.IsNullOrWhiteSpace(job.Description))
        {
            logger.LogWarning("Job with ID {JobId} has missing company name or description, skipping.", job.JobId);
            return false;
        }

        // Check if the job is remote
        if (job.Location.Contains("remote", StringComparison.CurrentCultureIgnoreCase))
        {
            logger.LogWarning("Job with ID {JobId} is remote, skipping.", job.JobId);
            return false;
        }

        // Check if the job is in the specified area (North Carolina or South Carolina)
        if (!job.Location.Contains(", nc", StringComparison.CurrentCultureIgnoreCase) &&
            !job.Location.Contains(", sc", StringComparison.CurrentCultureIgnoreCase))
        {
            logger.LogWarning("Job with ID {JobId} is not in area, skipping.", job.JobId);
            return false;
        }

        // This is to catch Data Center Engineer, etc
        if (job.Title.Contains("center", StringComparison.CurrentCultureIgnoreCase))
        {
            logger.LogWarning("Job with ID {JobId} is not relevant, skipping.", job.JobId);
            return false;
        }

        // Check if the job is coming from a recruitment company
        if (_recruitingCompanies.Any(c => job.CompanyName.Contains(c, StringComparison.CurrentCultureIgnoreCase)))
        {
            logger.LogWarning("Job with ID {JobId} is a possible recruitment company, skipping.", job.JobId);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Updates the names and titles within the organizational hierarchy results.
    /// </summary>
    /// <remarks>This method trims whitespace from the titles of all items in the hierarchy and replaces
    /// certain names with "Unknown" based on predefined lists of pronouns, conjunctions, and known words. The input
    /// <paramref name="results"/> must not be null, and its <see cref="HierarchyResults.OrgHierarchy"/> property must
    /// contain valid items to process.</remarks>
    /// <param name="results">The organizational hierarchy results to update. Cannot be null.</param>
    /// <returns>The updated <see cref="HierarchyResults"/> object with modified names and trimmed titles.</returns>
    public static HierarchyResults UpdateName(HierarchyResults results)
    {
        if (results is null)
        {
            return new();
        }

        if (results.OrgHierarchy is null || results.OrgHierarchy.Count == 0)
        {
            return results;
        }

        List<HierarchyItem> newResults = [];
        foreach (var item in results.OrgHierarchy ?? [])
        {
            if (item is null ||
                string.IsNullOrWhiteSpace(item.Name) ||
                string.IsNullOrWhiteSpace(item.Title) ||
                _invalidNames.Any(w => item.Name.Equals(w, StringComparison.CurrentCultureIgnoreCase)))
            {
                continue;
            }

            item.Title = item.Title.Trim();
            var nameWords = item.Name.Split([' ', '&'], StringSplitOptions.RemoveEmptyEntries);
            if (
                nameWords.Any(w => _pronouns.Contains(w, StringComparer.CurrentCultureIgnoreCase)) ||
                nameWords.Any(w => _conjunctions.Contains(w, StringComparer.CurrentCultureIgnoreCase)) ||
                nameWords.Any(w => _wordsNotInNames.Contains(w, StringComparer.CurrentCultureIgnoreCase))
            )
            {
                continue;
            }
            newResults.Add(item);
        }
        results.OrgHierarchy = newResults;
        return results;
    }

    /// <summary>
    /// Retrieves the organizational hierarchy relevant to a job posting by analyzing the job description  and the
    /// company's public organizational structure.
    /// </summary>
    /// <remarks>This method uses AI-based analysis to identify the reporting chain or leadership team
    /// responsible  for hiring the specified role. It prioritizes the closest manager, director, and relevant VP or 
    /// practice lead, while excluding unrelated executives.  If cached results are available for the specified job and
    /// organizational structure, they are returned  to improve performance. Otherwise, the method performs a real-time
    /// analysis using AI services.  The returned hierarchy includes names and titles that match the functional domain
    /// described in the job  posting. Titles unrelated to the hiring scope (e.g., CFO, CMO) are omitted.</remarks>
    /// <param name="job">The job posting details, including the company name and job description.</param>
    /// <param name="organicResults">The company's public organizational structure data.</param>
    /// <returns>A <see cref="HierarchyResults"/> object containing the relevant organizational hierarchy, or  <see
    /// langword="null"/> if no valid results are found.</returns>
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
                if (result is null || result.OrgHierarchy is null || result.OrgHierarchy.Count == 0)
                {
                    _logger.LogWarning("No valid organizational hierarchy found for company: {CompanyName}", job.CompanyName);
                    return null;
                }
                result = UpdateName(result);
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