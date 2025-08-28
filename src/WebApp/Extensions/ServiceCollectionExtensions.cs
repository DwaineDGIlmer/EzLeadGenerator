using Application.Configurations;
using Application.Constants;
using Application.Contracts;
using Application.Logging;
using Application.Models;
using Application.Services;
using Azure.Storage.Blobs;
using Core.Configuration;
using Core.Contracts;
using Core.Extensions;
using Core.Services;
using Loggers.Models;
using WebApp.Respository;

namespace WebApp.Extensions;

/// <summary>
/// Provides extension methods for adding services related to company profile storage to an <see
/// cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure-based logging services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method registers the <see cref="AzureLoggerProvider"/> as a singleton <see
    /// cref="ILoggerProvider"/> in the dependency injection container. It assumes that the required dependencies, such
    /// as <see cref="ICacheBlobClient"/>, <see cref="ILogEvent"/>, and <see cref="IOptions{TOptions}"/> for <see
    /// cref="EzLeadSettings"/>, are already registered in the service collection.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the Azure logging services will be added.</param>
    /// <param name="configuration">The application's configuration source used to retrieve ezSettings for the logger provider.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddAzureLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind EzLeadSettings from configuration
        var settingsSection = configuration.GetSection(nameof(EzLeadSettings));
        services.Configure<EzLeadSettings>(settingsSection);

        // Create an instance to validate
        var ezSettings = new EzLeadSettings();
        settingsSection.Bind(ezSettings);

        // Configure memory cache settings for logging
        var memSettings = Core.Extensions.ServiceCollectionExtensions.GetMemoryCacheSettings(configuration);
        memSettings.BlobName = ezSettings.LoggingBlobName;
        memSettings.Container = ezSettings.LoggingContainerName;
        memSettings.Prefix = ezSettings.LoggingPrefix;

        services.AddSingleton<ILoggerProvider, AzureLoggerProvider>(sp =>
        {
            ILogEvent logEventFactory() => sp.GetService<ILogEvent>() ?? new OtelLogEvents();
            var options = sp.GetRequiredService<IOptions<EzLeadSettings>>();

            // We want to use a BlobClient specifically for the log provider.
            var blobClient = new BlobCachingService(new BlobServiceClient(memSettings.AccountUrl), Options.Create(memSettings));
            return new AzureLoggerProvider(logEventFactory, blobClient, options);
        });

        services.AddSingleton(new BlobContainerClient(memSettings.AccountUrl, Defaults.LoggingContainerName));
        services.AddScoped<LogBlobReaderService>();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter<AzureLoggerProvider>("", LogLevel.Information);
        });

        return services;
    }

    /// <summary>
    /// Adds a local company profile store to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the store to.</param>
    /// <param name="configuration">Configuration ezSettings for the application, used to retrieve Azure ezSettings and connection strings.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCompanyProfileStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settingsSection = configuration.GetSection(nameof(AzureSettings));
        var settings = new AzureSettings();
        settingsSection.Bind(settings);
        services.Configure<AzureSettings>(configuration.GetSection(nameof(AzureSettings)));

        var env = Environment.GetEnvironmentVariable(Defaults.AspNetCoreEnvironment) ?? "Production";
        if (!env.Equals("Production"))
        {
            services.AddSingleton<ICompanyRepository>(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                var logger = sp.GetRequiredService<ILogger<LocalCompanyProfileStore>>();
                var options = sp.GetRequiredService<IOptions<EzLeadSettings>>();
                var cachingService = sp.GetRequiredService<ICacheService>();

                return new LocalCompanyProfileStore(cachingService, options, logger);
            });
        }
        else
        {
            services.AddSingleton<ICompanyRepository>(sp =>
            {
                // Keeping the configuration builder to add user secrets
                var builder = new ConfigurationBuilder();
                builder.AddConfiguration(configuration);
                var config = builder.AddUserSecrets<Program>().Build();

                var logger = sp.GetRequiredService<ILogger<AzureCompanyRepository>>();
                var cachingService = sp.GetRequiredService<ICacheService>();
                var options = sp.GetRequiredService<IOptions<AzureSettings>>();
                var ezSettings = sp.GetRequiredService<IOptions<EzLeadSettings>>();
                var connectionString = config.GetConnectionString("AzureTableStorage");
                var tableName = string.IsNullOrEmpty(settings.CompanyProfileTableName) ?
                Defaults.CompanyProfileTableName : settings.CompanyProfileTableName;
                var tabl = new Azure.Data.Tables.TableClient(connectionString, tableName);

                return new AzureCompanyRepository(tabl, cachingService, options, ezSettings, logger);
            });
        }
        return services;
    }

    /// <summary>
    /// Adds the appropriate jobs profile store to the service collection based on the current environment.
    /// </summary>
    /// <remarks>In non-production environments, a local jobs repository store is added. In
    /// production, an Azure company repository is used.</remarks>
    /// <param name="services">The service collection to which the jobs profile store will be added.</param>
    /// <param name="configuration">Configuration ezSettings for the application, used to retrieve Azure ezSettings and connection strings.</param>
    /// <returns>The updated service collection with the jobs profile store configured.</returns>
    public static IServiceCollection AddJobsProfileStore(
       this IServiceCollection services, IConfiguration configuration)
    {
        var settingsSection = configuration.GetSection(nameof(AzureSettings));
        var settings = new AzureSettings();
        settingsSection.Bind(settings);
        services.Configure<AzureSettings>(configuration.GetSection(nameof(AzureSettings)));

        var env = Environment.GetEnvironmentVariable(Defaults.AspNetCoreEnvironment) ?? "Production";
        if (!env.Equals("Production"))
        {
            services.AddSingleton<IJobsRepository>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<LocalJobsRepositoryStore>>();
                var options = sp.GetRequiredService<IOptions<EzLeadSettings>>();
                var cachingService = sp.GetRequiredService<ICacheService>();

                return new LocalJobsRepositoryStore(options, cachingService, logger);
            });
        }
        else
        {
            services.AddSingleton<IJobsRepository>(sp =>
            {
                // Keeping the configuration builder to add user secrets
                var builder = new ConfigurationBuilder();
                builder.AddConfiguration(configuration);

                var config = builder.AddUserSecrets<Program>().Build();
                var cachingService = sp.GetRequiredService<ICacheService>();
                var logger = sp.GetRequiredService<ILogger<AzureJobsRepository>>();
                var azSettings = sp.GetRequiredService<IOptions<AzureSettings>>();
                var ezSettings = sp.GetRequiredService<IOptions<EzLeadSettings>>();
                var connectionString = config.GetConnectionString("AzureTableStorage");
                var tableName = string.IsNullOrEmpty(settings.JobSummaryTableName) ?
                Defaults.JobSummaryTableName : settings.JobSummaryTableName;
                var tbl = new Azure.Data.Tables.TableClient(connectionString, tableName);

                return new AzureJobsRepository(tbl, cachingService, azSettings, ezSettings, logger);
            });
        }
        return services;
    }

    /// <summary>
    /// Configures the SerpApi ezSettings for the application by binding configuration values and applying environment
    /// variable overrides.
    /// </summary>
    /// <remarks>This method binds the SerpApi ezSettings from the application's configuration and applies
    /// default values and environment variable overrides where applicable. It ensures that the ezSettings are properly
    /// configured for use in the application.  The method expects a configuration section named <c>SerpApiSettings</c>
    /// to exist in the application's configuration. If certain values are missing, defaults or environment variables
    /// may be used.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the SerpApi ezSettings will be added.</param>
    /// <param name="configuration">The application's configuration source used to retrieve SerpApi ezSettings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection ConfigureSerpApiSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SerpApiSettings>(options =>
        {
            // Bind configuration values to azSettings
            configuration.GetSection(nameof(SerpApiSettings)).Bind(options);

            var settings = configuration.GetSection(nameof(AzureSettings));
            options.CacheExpirationInMinutes = settings.GetValue(nameof(EzLeadSettings.SerpApiQueryExpirationInMinutes), Defaults.SerpApiQueryExpirationInMinutes);

            // Apply environment variable and default overrides
            if (options.IsEnabled)
            {
                options.BaseAddress = string.IsNullOrEmpty(options.BaseAddress) ? Core.Constants.Defaults.SerpApiBaseAddress : options.BaseAddress;
                options.Endpoint = string.IsNullOrEmpty(options.BaseAddress) ? Defaults.SearchEndpoint : options.Endpoint;
                options.ApiKey = Environment.GetEnvironmentVariable(Defaults.EnvSearchApiKey) ?? options.ApiKey;
            }
        });
        return services;
    }

    /// <summary>
    /// Adds the jobs retrieval service and its dependencies to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method configures the <see cref="SerpApiSettings"/> from the application's
    /// configuration and registers the <see cref="SerpApiSearchJobsService"/> as a singleton service. It also sets up a
    /// resilient HTTP client with the specified base address from the ezSettings.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the service is added.</param>
    /// <param name="configuration">The application's configuration, used to retrieve ezSettings for the service.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the jobs retrieval service registered.</returns>
    public static IServiceCollection AddJobsRetrivalService(this IServiceCollection services, IConfiguration configuration)
    {
        var settingsSection = configuration.GetSection(nameof(SerpApiSettings));
        var settings = new SerpApiSettings();
        settingsSection.Bind(settings);

        services.AddResilientHttpClient(configuration, settings.HttpClientName, null, client =>
        {
            client.BaseAddress = new Uri(settings.BaseAddress);
        });
        services.AddSingleton<IJobsRetrieval<JobResult>, SerpApiSearchJobsService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SerpApiSettings>>();
            var cacheService = sp.GetRequiredService<ICacheService>();
            var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var logger = sp.GetRequiredService<ILogger<SerpApiSearchJobsService>>();
            return new SerpApiSearchJobsService(
                cacheService,
                clientFactory,
                options,
                logger);
        });
        return services;
    }

    /// <summary>
    /// Adds the <see cref="IDisplayRepository"/> service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the service is added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the <see cref="IDisplayRepository"/> service registered.</returns>
    public static IServiceCollection AddDisplayRepository(this IServiceCollection services)
    {
        services.AddSingleton<IDisplayRepository, DisplayRepository>();
        return services;
    }

    /// <summary>
    /// Adds the <see cref="IDisplayRepository"/> service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the service is added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the <see cref="IDisplayRepository"/> service registered.</returns>
    public static IServiceCollection AddJobSourceService(this IServiceCollection services)
    {
        services.AddSingleton<IJobSourceService, SearpApiSourceService>();
        services.AddSingleton<IAiChatService, OpenAiChatService>();
        services.AddSingleton<IOpenAiChatService, OpenAiChatService>();
        return services;
    }

    /// <summary>
    /// Adds the <see cref="IDisplayRepository"/> service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the service is added.</param>
    /// 
    /// <returns>The updated <see cref="IServiceCollection"/> with the <see cref="IDisplayRepository"/> service registered.</returns>
    public static IServiceCollection AddSearchService(this IServiceCollection services)
    {
        services.AddSingleton<ISearch, SerpApiSearchService>();
        return services;
    }
}
