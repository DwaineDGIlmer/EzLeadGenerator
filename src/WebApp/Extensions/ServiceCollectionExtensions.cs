using Application.Configurations;
using Application.Constants;
using Application.Contracts;
using Application.Models;
using Application.Services;
using Core.Caching;
using Core.Configuration;
using Core.Constants;
using Core.Contracts;
using Core.Extensions;
using Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WebApp.Respository;

namespace WebApp.Extensions;

/// <summary>
/// Provides extension methods for adding services related to company profile storage to an <see
/// cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a local company profile store to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the store to.</param>
    /// <param name="configuration">Configuration settings for the application, used to retrieve Azure settings and connection strings.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCompanyProfileStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settingsSection = configuration.GetSection(nameof(AzureSettings));
        var settings = new AzureSettings();
        settingsSection.Bind(settings);
        services.Configure<AzureSettings>(configuration.GetSection(nameof(AzureSettings)));

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (!env.Equals("Production"))
        {
            services.AddSingleton<ICompanyRepository>(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                var logger = sp.GetRequiredService<ILogger<LocalCompanyProfileStore>>();
                var options = sp.GetRequiredService<IOptions<SerpApiSettings>>();

                return new LocalCompanyProfileStore(options, logger);
            });
        }
        else
        {
            services.AddSingleton<ICompanyRepository>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AzureCompanyRepository>>();
                var options = sp.GetRequiredService<IOptions<AzureSettings>>();
                var connectionString = configuration.GetConnectionString("AzureTableStorage");
                var tableName = string.IsNullOrEmpty(settings.CompanyProfileTableName) ?
                Application.Constants.Defaults.CompanyProfileTableName : settings.CompanyProfileTableName;
                var tabl = new Azure.Data.Tables.TableClient(connectionString, tableName);

                return new AzureCompanyRepository(tabl, options, logger);
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
    /// <param name="configuration">Configuration settings for the application, used to retrieve Azure settings and connection strings.</param>
    /// <returns>The updated service collection with the jobs profile store configured.</returns>
    public static IServiceCollection AddJobsProfileStore(
       this IServiceCollection services, IConfiguration configuration)
    {
        var settingsSection = configuration.GetSection(nameof(AzureSettings));
        var settings = new AzureSettings();
        settingsSection.Bind(settings);
        services.Configure<AzureSettings>(configuration.GetSection(nameof(AzureSettings)));

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (!env.Equals("Production"))
        {
            services.AddSingleton<IJobsRepository>(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                var logger = sp.GetRequiredService<ILogger<LocalJobsRepositoryStore>>();
                var options = sp.GetRequiredService<IOptions<SerpApiSettings>>();

                return new LocalJobsRepositoryStore(options, logger);
            });
        }
        else
        {
            services.AddSingleton<IJobsRepository>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AzureJobsRepository>>();
                var options = sp.GetRequiredService<IOptions<AzureSettings>>();
                var connectionString = configuration.GetConnectionString("AzureTableStorage");
                var tableName = string.IsNullOrEmpty(settings.JobSummaryTableName) ?
                Application.Constants.Defaults.JobSummaryTableName : settings.JobSummaryTableName;
                var tbl = new Azure.Data.Tables.TableClient(connectionString, tableName);
                return new AzureJobsRepository(tbl, options, logger);
            });
        }
        return services;
    }

    /// <summary>
    /// Adds the jobs retrieval service and its dependencies to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method configures the <see cref="SerpApiSettings"/> from the application's
    /// configuration and registers the <see cref="SerpApiSearchJobsService"/> as a singleton service. It also sets up a
    /// resilient HTTP client with the specified base address from the settings.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the service is added.</param>
    /// <param name="configuration">The application's configuration, used to retrieve settings for the service.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the jobs retrieval service registered.</returns>
    public static IServiceCollection AddJobsRetrivalService(this IServiceCollection services, IConfiguration configuration)
    {
        var settingsSection = configuration.GetSection(nameof(SerpApiSettings));
        var settings = new SerpApiSettings();
        settingsSection.Bind(settings);

        services.Configure<SerpApiSettings>(options =>
        {
            // Bind configuration values to options
            configuration.GetSection(nameof(SerpApiSettings)).Bind(options);

            // Apply environment variable and default overrides
            if (options.IsEnabled)
            {
                options.BaseAddress = string.IsNullOrEmpty(options.BaseAddress) ? Core.Constants.Defaults.SerpApiBaseAddress : options.BaseAddress;
                options.Endpoint = string.IsNullOrEmpty(options.BaseAddress) ? Application.Constants.Defaults.SearchEndpoint : options.Endpoint;
                options.ApiKey = Environment.GetEnvironmentVariable("SEARCH_SERPAPI_API_KEY") ?? options.ApiKey ?? string.Empty;
            }
        });
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
                options,
                cacheService,
                clientFactory,
                logger);
        });
        return services;
    }

    /// <summary>
    /// Used to add file caching service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the service is added.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/>used for adding the services to.</param>
    /// <remarks>The name should be the class name.</remarks>
    /// <returns>IServiceCollection instance.</returns>
    public static IServiceCollection AddCacheService(this IServiceCollection services, IConfiguration configuration)
    {
        var settingsSection = configuration.GetSection(nameof(AiEventSettings));
        var settings = new AiEventSettings();
        settingsSection.Bind(settings);

        if (settings.CachingType == Core.Enums.CachingTypes.InMemory)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService>(sp =>
            {
                var cacheLogger = sp.GetRequiredService<ILogger<MemoryCacheService>>();
                var memoryCache = sp.GetRequiredService<IMemoryCache>();
                return new MemoryCacheService(memoryCache, true);
            });
        }
        if (settings.CachingType == Core.Enums.CachingTypes.FileSystem)
        {
            services.AddSingleton<ICacheService>(sp =>
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (dir is not null && !string.IsNullOrEmpty(dir.ToString()))
                {
                    settings.CacheLocation = Path.Combine(dir.ToString()!, "cache");
                }
                else
                {
                    settings.CacheLocation = Path.Combine(AppContext.BaseDirectory, "cache");
                }
                var cacheLogger = sp.GetRequiredService<ILogger<FileCacheService>>();
                return new FileCacheService(cacheLogger, settings.CacheLocation, true);
            });
        }
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
    /// <param name="configuration">The <see cref="IConfiguration"/>used for adding the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the <see cref="IDisplayRepository"/> service registered.</returns>
    public static IServiceCollection AddSearchService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SerpApiSettings>(options =>
        {
            configuration.GetSection(nameof(SerpApiSettings)).Bind(options);
            options.ApiKey = Environment.GetEnvironmentVariable(DefaultConstants.QDRANT_SERVICE_API_KEY) ?? options.ApiKey;
        });
        services.AddSingleton<ISearch<OrganicResult>, SerpApiSearchService>();
        return services;
    }

    /// <summary>
    /// Configures the application to use the job source service.
    /// </summary>
    /// <remarks>This method ensures that the job source service is registered and performs initial
    /// updates to the job source and company profiles. It throws an exception if the service is not
    /// available.</remarks>
    /// <param name="app">The application builder used to configure the application's request pipeline.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance for further configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the job source service is not registered in the application's service collection.</exception>
    public static IApplicationBuilder UseJobSourceService(this IApplicationBuilder app)
    {
        var job = app.ApplicationServices.GetService<IJobSourceService>() ?? throw new InvalidOperationException("Job source service is not registered.");
        job.UpdateJobSourceAsync().GetAwaiter().GetResult();
        job.UpdateCompanyProfilesAsync().GetAwaiter().GetResult();
        return app;
    }
}
