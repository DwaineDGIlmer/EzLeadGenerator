using Application.Configurations;
using Application.Contracts;
using Application.Models;
using Core.Contracts;
using Core.Helpers;
using System.Reflection;
using System.Text.Json.Serialization;
using WebApp.Extensions;

namespace WebApp.Respository;

/// <summary>
/// Local development repository for company profiles and company summaries using JSON files.
/// </summary>
public sealed class LocalCompanyProfileStore : ICompanyRepository
{
    private readonly string _companyProfileDirectory;
    private readonly int _cacheExpirationDays;
    private readonly ICacheService _cachingService;
    private readonly ILogger<LocalCompanyProfileStore> _logger;
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new JsonStringEnumConverter(), new DateTimeConverter() }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalCompanyProfileStore"/> class.
    /// </summary>
    /// <remarks>This constructor ensures that the specified directories exist by creating them if
    /// they do not.</remarks>
    /// <param name="cacheService">The cache service used for caching operations. Cannot be <see langword="null"/>.</param>
    /// <param name="options">Configuration options for the service, including paths for profile and company directories. Cannot be <see langword="null"/>.</param>
    /// <param name="logger">The logger instance used for logging operations within the store. Cannot be <see langword="null"/>.</param>
    public LocalCompanyProfileStore(
        ICacheService cacheService,
        IOptions<EzLeadSettings> options,
        ILogger<LocalCompanyProfileStore> logger)
    {
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        _cachingService = cacheService;
        _cacheExpirationDays = options.Value.CompanyCacheExpirationInDays;
        _companyProfileDirectory = GetProfileDirectory(options.Value).Replace('/', '\\');
        _logger = logger;

        // This is the same directory used by any caller using SerpApiSettings
        Directory.CreateDirectory(_companyProfileDirectory);
    }

    /// <summary>
    /// Asynchronously retrieves the company profile for the specified company ID.
    /// </summary>
    /// <remarks>This method reads the company profile from a JSON file located in a predefined
    /// directory. If the file is not found or the data cannot be deserialized, appropriate exceptions are
    /// thrown.</remarks>
    /// <param name="companyId">The unique identifier of the company whose profile is to be retrieved. Cannot be null or empty.</param>
    /// <returns>A <see cref="CompanyProfile"/> object representing the company's profile.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the profile file for the specified <paramref name="companyId"/> does not exist.</exception>
    public async Task<CompanyProfile?> GetCompanyProfileAsync(string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));

        _logger.LogInformation("Retrieving company profile for {companyId}", companyId);

        var cacheProfile = await _cachingService.GetCompanyAsync(companyId, _logger);
        if (cacheProfile is not null)
        {
            return cacheProfile;
        }

        string path = GetFilePath(companyId);
        if (!File.Exists(path))
        {
            _logger.LogWarning("Profile not found for companyId: {companyId}", companyId);
            return null;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            var profile = await JsonSerializer.DeserializeAsync<CompanyProfile>(stream, _options);
            if (profile is not null)
            {
                _logger.LogInformation("Successfully retrieved company profile for {companyId}", companyId);

                await _cachingService.AddCompanyAsync(profile, _cacheExpirationDays);
                _logger.LogDebug("Caching all company profile, company id: {companyId}", companyId);
            }
            return profile ?? null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load profile for companyId: {companyId}", companyId);
            throw new IOException($"Profile file not found for companyId: {companyId}", ex);
        }
    }

    /// <summary>
    /// Asynchronously retrieves a list of company profiles created on or after the specified date.
    /// </summary>
    /// <remarks>This method reads JSON files from a predefined directory, deserializes them into company
    /// profiles, and filters the results based on the provided date. If a file cannot be deserialized, an error is
    /// logged, and the file is skipped.</remarks>
    /// <param name="fromDate">The date from which to filter company profiles. Only profiles created on or after this date are included.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of <see cref="CompanyProfile"/>
    /// objects that match the specified date criteria.</returns>
    public async Task<IEnumerable<CompanyProfile>> GetCompanyProfileAsync(DateTime fromDate)
    {
        var cachedProfiles = await _cachingService.GetCompaniesAsync(fromDate, _logger);
        if (cachedProfiles != null)
        {
            return cachedProfiles;
        }

        var allCompanies = new List<CompanyProfile>();
        foreach (var file in Directory.GetFiles(_companyProfileDirectory, "*.json"))
        {
            try
            {
                using FileStream stream = File.OpenRead(file);
                using StreamReader reader = new(stream);
                string jsonContent = await reader.ReadToEndAsync();
                _logger.LogInformation("JSON Content: {JsonContent}", jsonContent);

                var company = JsonSerializer.Deserialize<CompanyProfile>(jsonContent, _options);
                if (company != null && company.CreatedAt >= fromDate)
                    allCompanies.Add(company);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error in file: {File}", file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading company summary file: {File}", file);
                throw new IOException($"Error reading company summary file: {file}", ex);
            }
        }

        if (allCompanies.Count != 0)
        {
            _logger.LogDebug("Caching all company profiles with count: {Count}", allCompanies.Count);
            await _cachingService.AddCompaniesAsync(allCompanies, fromDate, _cacheExpirationDays);
        }
        return allCompanies;
    }

    /// <summary>
    /// Asynchronously saves the specified company profile to a JSON file.
    /// </summary>
    /// <remarks>The profile is saved in the directory specified by the internal profile directory
    /// path, with the filename based on the company's ID. If an error occurs during the save operation, the
    /// exception is logged and rethrown.</remarks>
    /// <param name="profile">The company profile to save. Cannot be <see langword="null"/>.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profile"/> is <see langword="null"/>.</exception>
    public async Task AddCompanyProfileAsync(CompanyProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile, nameof(profile));
        string path = GetFilePath(profile.CompanyId);

        try
        {
            using FileStream stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, profile, _options);
            _logger.LogInformation("Profile saved: {companyId}", profile.CompanyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profile: {companyId}", profile.CompanyId);
            throw new IOException($"Failed to save profile for companyId: {profile.CompanyId}", ex);
        }
    }

    /// <summary>
    /// Updates the specified company profile asynchronously, saving changes to persistent storage.
    /// </summary>
    /// <remarks>If the company profile does not exist in storage, a new profile is added. The method updates
    /// all writable properties of the existing profile with the values from the provided profile, except for properties
    /// that should be ignored based on custom logic. The <c>UpdatedAt</c> timestamp is always refreshed to the current
    /// UTC time.</remarks>
    /// <param name="profile">The <see cref="CompanyProfile"/> object containing the updated company information. Cannot be <see
    /// langword="null"/>.</param>
    /// <returns></returns>
    public async Task UpdateCompanyProfileAsync(CompanyProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile, nameof(profile));
        if (string.IsNullOrWhiteSpace(profile.CompanyId))
            throw new ArgumentException("Company ID cannot be null or empty.", nameof(profile));

        try
        {
            string path = GetFilePath(profile.CompanyId);
            if (!File.Exists(path))
            {
                await AddCompanyProfileAsync(profile);
                return;
            }

            // Should not happen, but just in case
            var jsonProfile = await UpdateProperties(profile, _companyProfileDirectory, _logger);
            if (jsonProfile == null)
            {
                _logger.LogWarning("Profile not found for update: {companyId}", profile.CompanyId);
                return;
            }

            // Update the profile with the properties from the existing JSON profile
            await AddCompanyProfileAsync(jsonProfile);
            _logger.LogInformation("Profile updated: {Id}", profile.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profile: {Id}", profile.Id);
            throw new IOException($"Failed to save profile for companyId: {profile.CompanyId}", ex);
        }
    }

    /// <summary>
    /// Asynchronously deletes the specified company profile from the storage.
    /// </summary>
    /// <remarks>This method deletes the company profile file associated with the given <paramref
    /// name="profile"/> from the designated storage directory. If the file does not exist, no action is taken. Any
    /// exceptions encountered during the deletion process are logged.</remarks>
    /// <param name="profile">The company profile to delete. Cannot be <see langword="null"/>.</param>
    /// <returns></returns>
    public async Task DeleteCompanyProfileAsync(CompanyProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile, nameof(profile));
        string path = Path.Combine(_companyProfileDirectory, $"{profile.CompanyId}.json");
        await Task.Run(() =>
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    _logger.LogInformation("Company profile deleted: {companyId}", profile.CompanyId);
                }
                else
                {
                    _logger.LogWarning("Company profile not found for deletion: {companyId}", profile.CompanyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Company profile: {companyId}", profile.CompanyId);
                throw new IOException($"Failed to delete profile for companyId: {profile.CompanyId}", ex);
            }
        });
    }

    /// <summary>
    /// Updates the properties of a <see cref="CompanyProfile"/> instance based on the values from an existing profile.
    /// </summary>
    /// <remarks>This method updates writable properties of the provided <paramref name="profile"/> based on
    /// the corresponding values in the existing profile loaded from the specified file. Properties marked for ignoring,
    /// as determined by the  <see cref="ReflectionHelper.ShouldIgnoreProperty"/> method, will not be updated.
    /// Additionally, the <c>UpdatedAt</c>  property of the <paramref name="profile"/> is always set to the current UTC
    /// timestamp if it exists and is writable.</remarks>
    /// <param name="profile">The <see cref="CompanyProfile"/> instance whose properties will be updated.</param>
    /// <param name="profileDirectory">The directory where the profile file is located. Must not be null or empty.</param>  
    /// <param name="logger">The logger instance used for logging errors and information. Cannot be <see langword="null"/>.</param>
    /// <returns>The original <see cref="CompanyProfile"/> instance deserialized from the file, or <see langword="null"/> if the
    /// file does not contain valid data.</returns>
    public static async Task<CompanyProfile?> UpdateProperties(CompanyProfile profile, string profileDirectory, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(profile, nameof(profile));
        ArgumentNullException.ThrowIfNull(profileDirectory, nameof(profileDirectory));

        if (!Directory.Exists(profileDirectory))
        {
            throw new DirectoryNotFoundException($"The directory '{profileDirectory}' does not exist.");
        }

        CompanyProfile? jsonProfile = null;
        try
        {
            var path = GetFilePath(profile.CompanyId, profileDirectory);
            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);

            jsonProfile = await JsonSerializer.DeserializeAsync<CompanyProfile>(stream);
            if (jsonProfile is not null)
            {
                // Ensure the OrgHierarchy is not null and contains unique items
                foreach (var item in profile.HierarchyResults.OrgHierarchy)
                {
                    if (item is not null &&
                        !jsonProfile.HierarchyResults.OrgHierarchy.ToList().Exists(x => x.Name == item.Name))
                    {
                        jsonProfile.HierarchyResults.OrgHierarchy.Add(item);
                    }
                }

                var properties = typeof(CompanyProfile).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    // Skip read-only properties and properties that should not be updated
                    if (!property.CanWrite || !property.CanRead || property.Name == nameof(CompanyProfile.HierarchyResults))
                        continue;

                    var incomingValue = property.GetValue(profile);
                    var existingValue = property.GetValue(jsonProfile);

                    // Skip if incoming value should be ignored
                    if (ReflectionHelper.ShouldIgnoreProperty(property, incomingValue))
                        continue;

                    // Update the property
                    property.SetValue(profile, incomingValue);
                }
            }
        }
        catch (JsonException jsonEx)
        {
            // Log the error and return null if deserialization fails
            logger.LogError(jsonEx, "JSON deserialization error: {Message}", jsonEx.Message);
            return null;
        }
        catch (Exception ex)
        {
            // Log any other errors that occur
            logger.LogError(ex, "Error reading company profile file: {Message}", ex.Message);
            throw new IOException($"Error reading company profile file: {profile.CompanyId}", ex);
        }

        // Always update the UpdatedAt timestamp if the property exists
        var updatedAtProperty = typeof(CompanyProfile).GetProperty(nameof(CompanyProfile.UpdatedAt));
        if (updatedAtProperty != null && updatedAtProperty.CanWrite)
        {
            updatedAtProperty.SetValue(profile, DateTime.UtcNow);
        }
        return jsonProfile ?? profile;
    }

    /// <summary>
    /// Retrieves the file path associated with the specified company ID.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company. Cannot be null or empty.</param>
    /// <returns>The file path corresponding to the given company ID.</returns>
    public string GetFilePath(string companyId)
    {
        return GetFilePath(companyId, _companyProfileDirectory);
    }

    /// <summary>
    /// Retrieves the file path associated with the specified company ID.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company. Cannot be null or empty.</param>
    /// <param name="profileDirectory">The directory where the profile files are stored. Cannot be null or empty.</param>
    /// <returns>The file path corresponding to the given company ID.</returns>
    public static string GetFilePath(string companyId, string profileDirectory) =>
    Path.Combine(profileDirectory, $"company.{companyId.FileSystemName()}.json");

    private static string GetProfileDirectory(EzLeadSettings? settings)
    {
        if (settings is not null && !string.IsNullOrEmpty(settings.FileCompanyProfileDirectory))
        {
            return Path.Combine(settings.FileCompanyProfileDirectory, "companies");
        }

        var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (dir is not null && !string.IsNullOrEmpty(dir.ToString()))
        {
            return Path.Combine(dir.ToString(), "cache", "companies");
        }
        return Path.Combine(AppContext.BaseDirectory, "cache", "companies");
    }
}