using Application.Configurations;
using Application.Constants;
using Application.Contracts;
using Application.Models;
using Azure;
using Azure.Data.Tables;
using Core.Contracts;
using WebApp.Extensions;

namespace WebApp.Respository;

/// <summary>
/// Provides an implementation of <see cref="ICompanyRepository"/> that uses Azure Table Storage and Blob Storage.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AzureCompanyRepository"/> class with the specified table
/// client, blob container client, and logger.
/// </remarks>
/// <param name="tableClient">The <see cref="TableClient"/> used to interact with the Azure Table storage.</param>
/// <param name="cachingService">The <see cref="ICacheService"/> used for caching company profiles.</param>
/// <param name="azSettings">Configuration settings from Azure settings, including the table name.</param>
/// <param name="ezSettings">Configuration settings from EzLead settings.</param>
/// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance used for logging operations within the repository.</param>
public class AzureCompanyRepository(
    TableClient tableClient,
    ICacheService cachingService,
    IOptions<AzureSettings> azSettings,
    IOptions<EzLeadSettings> ezSettings,
    ILogger<AzureCompanyRepository> logger) : ICompanyRepository
{
    private readonly TableClient _tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
    private readonly ICacheService _cachingService = cachingService ?? throw new ArgumentNullException(nameof(cachingService));
    private readonly ILogger<AzureCompanyRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly string _partionKey = azSettings?.Value?.CompanyProfilePartionKey ?? Defaults.CompanyProfilePartionKey;
    private readonly int _cacheExpirationInDays = ezSettings?.Value?.CompanyCacheExpirationInDays ?? Defaults.CompanyCacheExpirationInDays;
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Asynchronously retrieves the company profile for the specified company ID.
    /// </summary>
    /// <param name="CompanyId">The unique identifier of the company whose profile is to be retrieved. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="CompanyProfile"/>
    /// of the specified company.</returns>
    /// <exception cref="ArgumentException">Thrown when CompanyId is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the deserialized profile is null.</exception>
    public async Task<CompanyProfile?> GetCompanyProfileAsync(string CompanyId)
    {
        if (string.IsNullOrWhiteSpace(CompanyId))
            throw new ArgumentException("Company ID cannot be null or empty.", nameof(CompanyId));

        _logger.LogInformation("Retrieving company profile for {CompanyId}", CompanyId);

        var cachedProfile = await _cachingService.GetCompanyAsync(CompanyId, _logger);
        if (cachedProfile != null)
        {
            return cachedProfile;
        }

        try
        {
            var response = await _tableClient.GetEntityAsync<TableEntity>(_partionKey, CompanyId);
            if (response is null)
            {
                _logger.LogInformation("Response from Tableclient was null: {CompanyId}", CompanyId);
                return null;
            }
            var json = response.Value.GetString("Data");

            if (string.IsNullOrEmpty(json))
            {
                _logger.LogInformation("No data found for company name: {CompanyId}", CompanyId);
                return null;
            }

            var profile = JsonSerializer.Deserialize<CompanyProfile>(json, _options);
            if (profile is not null)
            {
                _logger.LogInformation("Successfully retrieved company profile for {CompanyId}", CompanyId);

                await _cachingService.AddCompanyAsync(profile, _cacheExpirationInDays);
                _logger.LogDebug("Added company profile for {CompanyId} to cache", CompanyId);
            }
            return profile ?? null;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogInformation("Company profile not found for {CompanyId}", CompanyId);
            return null;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error retrieving profile for {CompanyId}", CompanyId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving company profile for {CompanyId}", CompanyId);
            throw new InvalidOperationException($"An error occurred while retrieving the company profile for ID: {CompanyId}", ex);
        }
    }

    /// <summary>
    /// Asynchronously retrieves a paginated list of company profiles updated since a specified date.
    /// </summary>
    /// <param name="fromDate">The date from which to filter company profiles based on their last job synchronization date.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of <see
    /// cref="CompanyProfile"/> objects that match the specified criteria.</returns>
    public async Task<IEnumerable<CompanyProfile>> GetCompanyProfileAsync(DateTime fromDate)
    {
        _logger.LogInformation("Retrieving company profiles updated since {FromDate}", fromDate);

        var cachedProfiles = await _cachingService.GetCompaniesAsync(fromDate, _logger);
        if (cachedProfiles != null)
        {
            _logger.LogInformation("Retrieved company profiles from cache");
            return cachedProfiles;
        }

        var queryResults = _tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{_partionKey}'");
        var companies = new List<CompanyProfile>();

        await foreach (var entity in queryResults)
        {
            try
            {
                var json = entity.GetString("Data");
                if (string.IsNullOrEmpty(json)) continue;

                var profile = JsonSerializer.Deserialize<CompanyProfile>(json, _options);
                if (profile != null && profile.UpdatedAt >= fromDate)
                {
                    companies.Add(profile);
                }

                if (companies.Count != 0)
                {
                    _logger.LogDebug("Caching company profiles for partition key: {PartitionKey}", _partionKey);
                    await _cachingService.AddCompaniesAsync(companies, fromDate, _cacheExpirationInDays);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse company profile from Azure Table for entity with RowKey: {RowKey}", entity.RowKey);
                throw new InvalidOperationException($"An error occurred while parsing the company profile for RowKey: {entity.RowKey}", ex);
            }
        }

        _logger.LogInformation("Successfully retrieved {Count} company profiles updated since {FromDate}", companies.Count, fromDate);
        return companies;
    }

    /// <summary>
    /// Asynchronously saves the specified company profile to the data store.
    /// </summary>
    /// <param name="profile">The <see cref="CompanyProfile"/> object containing the company details to be saved. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when profile is null.</exception>
    public async Task AddCompanyProfileAsync(CompanyProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (string.IsNullOrWhiteSpace(profile.CompanyId))
            throw new ArgumentException("Company ID cannot be null or empty.", nameof(profile));

        try
        {
            _logger.LogInformation("Adding company profile for {CompanyId}", profile.CompanyId);

            var entity = new TableEntity(_partionKey, profile.CompanyId)
            {
                {"Data", JsonSerializer.Serialize(profile, _options)},
                {"Id", profile.Id ?? string.Empty},
                {"CompanyId", profile.CompanyId ?? string.Empty},
                {"CompanyName", profile.CompanyName ?? string.Empty},
                {"DomainName", profile.DomainName ?? string.Empty},
                {"Link", profile.Link ?? string.Empty},
                {"HierarchyResults", JsonSerializer.Serialize(profile.HierarchyResults, _options)},
                {"CreatedAt", profile.CreatedAt.ToString("o")},
                {"UpdatedAt", profile.UpdatedAt.ToString("o")}
            };

            await _tableClient.UpsertEntityAsync(entity);
            _logger.LogInformation("Successfully added company profile for {CompanyId}", profile.CompanyId);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error adding company profile for {CompanyId}", profile.CompanyId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error adding company profile for {CompanyId}", profile.CompanyId);
            throw new InvalidOperationException($"An error occurred while adding the company profile for ID: {profile.CompanyId}", ex);
        }
    }

    /// <summary>
    /// Updates the company information asynchronously based on the provided profile.
    /// </summary>
    /// <param name="profile">The <see cref="CompanyProfile"/> object containing the updated company details.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when profile is null.</exception>
    public async Task UpdateCompanyProfileAsync(CompanyProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (string.IsNullOrWhiteSpace(profile.CompanyId))
            throw new ArgumentException("Company ID cannot be null or empty.", nameof(profile));

        try
        {
            _logger.LogDebug("Updating company profile for {CompanyId}", profile.CompanyId);

            // Update the UpdatedAt timestamp
            profile.UpdatedAt = DateTime.UtcNow;

            var entity = new TableEntity(_partionKey, profile.CompanyId)
            {
                {"Id", profile.Id},
                {"Data", JsonSerializer.Serialize(profile, _options)},
                {"CompanyName", profile.CompanyName ?? string.Empty},
                {"CompanyId", profile.CompanyId ?? string.Empty},
                {"DomainName", profile.DomainName ?? string.Empty},
                {"Link", profile.Link ?? string.Empty},
                {"HierarchyResults", JsonSerializer.Serialize(profile.HierarchyResults, _options)},
                {"UpdatedAt", profile.UpdatedAt.ToString("o")},
                {"CreatedAt", profile.CreatedAt.ToString("o")}
            };

            await _tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
            _logger.LogInformation("Successfully updated company profile for {CompanyId}", profile.CompanyId);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Company profile not found for update: {CompanyId}", profile.CompanyId);
            throw new InvalidOperationException($"Company profile not found for ID: {profile.CompanyId}", ex);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error updating company profile for {CompanyId}", profile.CompanyId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating company profile for {CompanyId}", profile.CompanyId);
            throw new InvalidOperationException($"An error occurred while updating the company profile for ID: {profile.CompanyId}", ex);
        }
    }

    /// <summary>
    /// Asynchronously deletes the specified company profile.
    /// </summary>
    /// <param name="profile">The <see cref="CompanyProfile"/> representing the company to be deleted. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when profile is null.</exception>
    public async Task DeleteCompanyProfileAsync(CompanyProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (string.IsNullOrWhiteSpace(profile.CompanyId))
            throw new ArgumentException("Company ID cannot be null or empty.", nameof(profile));

        try
        {
            _logger.LogDebug("Deleting company profile for {CompanyId}", profile.CompanyId);

            await _tableClient.DeleteEntityAsync(_partionKey, profile.CompanyId, ETag.All);
            _logger.LogInformation("Successfully deleted company profile for {CompanyId}", profile.CompanyId);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Company profile not found for deletion: {CompanyId}", profile.CompanyId);
            // Consider whether to throw or silently succeed when entity doesn't exist
            // throw new InvalidOperationException($"Company profile not found for ID: {profile.CompanyId}", ex);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error deleting company profile for {CompanyId}", profile.CompanyId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting company profile for {CompanyId}", profile.CompanyId);
            throw new InvalidOperationException($"An error occurred while deleting the company profile for ID: {profile.CompanyId}", ex);
        }
    }
}