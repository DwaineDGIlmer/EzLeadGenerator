using Application.Contracts;
using Application.Extensions;
using Application.Models;
using Core.Configuration;
using Core.Helpers;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApp.Respository;

/// <summary>
/// Local development repository for company profiles and company summaries using JSON files.
/// </summary>
public class LocalCompanyProfileStore : ICompanyRepository
{
    private readonly string _companyProfileDirectory;
    private readonly ILogger<LocalCompanyProfileStore> _logger;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(), new DateTimeConverter() }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalCompanyProfileStore"/> class.
    /// </summary>
    /// <remarks>This constructor ensures that the specified directories exist by creating them if
    /// they do not.</remarks>
    /// <param name="options">Configuration options for the service, including paths for profile and company directories. Cannot be <see langword="null"/>.</param>
    /// <param name="logger">The logger instance used for logging operations within the store. Cannot be <see langword="null"/>.</param>
    public LocalCompanyProfileStore(
    IOptions<SerpApiSettings> options,
    ILogger<LocalCompanyProfileStore> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(options.Value.FileCompanyProfileDirectory, nameof(options.Value.FileCompanyProfileDirectory));

        _companyProfileDirectory = Path.Combine(Environment.CurrentDirectory, options.Value.FileCompanyProfileDirectory).Replace('/', '\\');
        _logger = logger;

        Directory.CreateDirectory(_companyProfileDirectory);
    }

    /// <summary>
    /// Asynchronously retrieves the company profile for the specified company ID.
    /// </summary>
    /// <remarks>This method reads the company profile from a JSON file located in a predefined
    /// directory. If the file is not found or the data cannot be deserialized, appropriate exceptions are
    /// thrown.</remarks>
    /// <param name="companyName">The unique identifier of the company whose profile is to be retrieved. Cannot be null or empty.</param>
    /// <returns>A <see cref="CompanyProfile"/> object representing the company's profile.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the profile file for the specified <paramref name="companyName"/> does not exist.</exception>
    public async Task<CompanyProfile?> GetCompanyProfileAsync(string companyName)
    {

        string path = Path.Combine(_companyProfileDirectory, $"{companyName.FileSystemName()}.json");
        if (!File.Exists(path))
        {
            _logger.LogWarning("Profile not found for companyName: {companyName}", companyName);
            return null;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            var profile = await JsonSerializer.DeserializeAsync<CompanyProfile>(stream, _options);
            return profile ?? null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load profile for companyName: {CompanyId}", companyName);
            throw;
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
            }
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
        string path = Path.Combine(_companyProfileDirectory, $"{profile.CompanyId}.json");

        try
        {
            using FileStream stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, profile, _options);
            _logger.LogInformation("Profile saved: {CompanyId}", profile.CompanyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profile: {CompanyId}", profile.CompanyId);
            throw;
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
        string path = Path.Combine(_companyProfileDirectory, $"{profile.CompanyId}.json");
        try
        {
            if (!File.Exists(path))
            {
                await AddCompanyProfileAsync(profile);
                return;
            }

            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            var jsonProfile = await JsonSerializer.DeserializeAsync<CompanyProfile>(stream);
            var properties = typeof(CompanyProfile).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // Skip read-only properties
                if (!property.CanWrite || !property.CanRead)
                    continue;

                var incomingValue = property.GetValue(profile);
                var existingValue = property.GetValue(jsonProfile);

                // Skip if incoming value should be ignored
                if (ReflectionHelper.ShouldIgnoreProperty(property, incomingValue))
                    continue;

                // Update the property
                property.SetValue(profile, incomingValue);
            }

            // Always update the UpdatedAt timestamp if the property exists
            var updatedAtProperty = typeof(CompanyProfile).GetProperty(nameof(CompanyProfile.UpdatedAt));
            if (updatedAtProperty != null && updatedAtProperty.CanWrite)
            {
                updatedAtProperty.SetValue(profile, DateTime.UtcNow);
            }

            _logger.LogInformation("Profile updated: {Id}", profile.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profile: {Id}", profile.Id);
            throw;
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
                    _logger.LogInformation("Company profile deleted: {CompanyId}", profile.CompanyId);
                }
                else
                {
                    _logger.LogWarning("Company profile not found for deletion: {CompanyId}", profile.CompanyId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Company profile: {CompanyId}", profile.CompanyId);
                throw;
            }
        });
    }
}