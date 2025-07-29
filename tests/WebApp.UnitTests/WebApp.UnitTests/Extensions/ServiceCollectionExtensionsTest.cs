using Application.Contracts;
using Application.Models;
using Core.Configuration;
using Core.Contracts;
using Core.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI;
using WebApp.Extensions;
using WebApp.Respository;

namespace WebApp.UnitTests.Extensions;

public class ServiceCollectionExtensionsTest
{
    private static IServiceCollection CreateServiceCollection()
    {
        return new ServiceCollection();
    }

    private static IConfiguration CreateConfiguration(Action<IConfigurationBuilder>? configure = null)
    {
        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection();
        configure?.Invoke(builder);
        return builder.Build();
    }

    [Fact]
    public void AddCompanyProfileStore_RegistersLocalCompanyProfileStore_WhenNotProduction()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var services = CreateServiceCollection();
        var configuration = CreateConfiguration();

        services.AddLogging();
        services.AddSingleton(Mock.Of<IWebHostEnvironment>());
        services.Configure<SerpApiSettings>(option =>
        {
            option = new SerpApiSettings();
        });

        services.AddCompanyProfileStore(configuration);

        var provider = services.BuildServiceProvider();
        var repo = provider.GetService<ICompanyRepository>();
        Assert.NotNull(repo);
    }

    [Fact]
    public void AddCompanyProfileStore_RegistersAzureCompanyRepository_WhenProduction()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var services = CreateServiceCollection();
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("ConnectionStrings:AzureTableStorage", "UseDevelopmentStorage=true"),
            new KeyValuePair<string, string?>("AzureSettings:CompanyProfileTableName", "TestTable")
        ]);
        var configuration = configBuilder.Build();

        services.AddLogging();
        services.AddCompanyProfileStore(configuration);

        var provider = services.BuildServiceProvider();
        var repo = provider.GetService<ICompanyRepository>();
        Assert.NotNull(repo);
    }

    [Fact]
    public void AddJobsProfileStore_RegistersLocalJobsRepositoryStore_WhenNotProduction()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var services = CreateServiceCollection();
        var configuration = CreateConfiguration();

        services.AddLogging();
        services.AddSingleton(Mock.Of<IWebHostEnvironment>());
        services.AddSingleton(Mock.Of<ISearch<OrganicResult>>());
        services.AddSingleton(Mock.Of<IJobsRetrieval<JobResult>>());
        services.AddSingleton(Mock.Of<IJobsRepository>());
        services.AddSingleton(Mock.Of<ICompanyRepository>());
        services.AddSingleton(Mock.Of<ICacheService>());
        services.AddSingleton(Mock.Of<IHttpClientFactory>());
        services.AddSingleton(Mock.Of<OpenAIClient>());
        services.AddSingleton(Mock.Of<ILogger<LocalJobsRepositoryStore>>());
        services.Configure<OpenAiSettings>(options =>
        {
            options = new OpenAiSettings();
        });
        services.Configure<SerpApiSettings>(options =>
        {
            options = new SerpApiSettings()
            {
                FileJobProfileDirectory = "TestDirectory"
            };
        });

        services.AddJobsProfileStore(configuration);

        var provider = services.BuildServiceProvider();
        var repo = provider.GetService<IJobsRepository>();
        Assert.NotNull(repo);
    }

    [Fact]
    public void AddJobsProfileStore_RegistersAzureJobsRepository_WhenProduction()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var services = CreateServiceCollection();
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("ConnectionStrings:AzureTableStorage", "UseDevelopmentStorage=true"),
            new KeyValuePair<string, string?>("AzureSettings:JobSummaryTableName", "TestTable")
        ]);
        var configuration = configBuilder.Build();

        services.AddLogging();
        services.AddJobsProfileStore(configuration);

        var provider = services.BuildServiceProvider();
        var repo = provider.GetService<IJobsRepository>();
        Assert.NotNull(repo);
    }

    [Fact]
    public void AddCacheService_RegistersMemoryCacheService_WhenInMemory()
    {
        var services = CreateServiceCollection();
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(
        [
            new KeyValuePair<string, string>("AiEventSettings:CachingType", ((int)CachingTypes.InMemory).ToString())!
        ]);
        var configuration = configBuilder.Build();

        services.AddLogging();
        services.AddCacheService(configuration);

        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();
        Assert.NotNull(cacheService);
    }

    [Fact]
    public void AddCacheService_RegistersFileCacheService_WhenFileSystem()
    {
        var services = CreateServiceCollection();
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("AiEventSettings:CachingType", ((int)CachingTypes.FileSystem).ToString())
        ]);
        var configuration = configBuilder.Build();

        services.AddLogging();
        services.AddCacheService(configuration);

        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();
        Assert.NotNull(cacheService);
    }

    [Fact]
    public void AddDisplayRepository_RegistersDisplayRepository()
    {
        var services = CreateServiceCollection();
        services.AddLogging();
        services.AddSingleton(Mock.Of<IJobsRepository>());
        services.AddSingleton(Mock.Of<ICompanyRepository>());
        services.AddDisplayRepository();

        var provider = services.BuildServiceProvider();
        var repo = provider.GetService<IDisplayRepository>();
        Assert.NotNull(repo);
    }

    [Fact]
    public void AddJobSourceService_RegistersJobSourceAndAiChatServices()
    {
        var services = CreateServiceCollection();
        services.AddLogging();
        services.AddSingleton(Mock.Of<ISearch<OrganicResult>>());
        services.AddSingleton(Mock.Of<IJobsRetrieval<JobResult>>());
        services.AddSingleton(Mock.Of<IJobsRepository>());
        services.AddSingleton(Mock.Of<ICompanyRepository>());
        services.AddSingleton(Mock.Of<ICacheService>());
        services.AddSingleton(Mock.Of<IHttpClientFactory>());
        services.AddSingleton(Mock.Of<OpenAIClient>());
        services.Configure<OpenAiSettings>(options =>
        {
            options = new OpenAiSettings();
        });
        services.Configure<SerpApiSettings>(options =>
        {
            options = new SerpApiSettings();
        });
        services.AddJobSourceService();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IJobSourceService>());
        Assert.NotNull(provider.GetService<IAiChatService>());
        Assert.NotNull(provider.GetService<IOpenAiChatService>());
    }

    [Fact]
    public void AddSearchService_RegistersSearchService()
    {
        var services = CreateServiceCollection();
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("SerpApiSettings:ApiKey", "1234567890")
        ]);
        var configuration = configBuilder.Build();
        services.AddLogging();
        services.AddSingleton(Mock.Of<IHttpClientFactory>());
        services.AddSingleton(Mock.Of<ICacheService>());
        services.AddSearchService(configuration);

        var provider = services.BuildServiceProvider();
        var search = provider.GetService<ISearch<Application.Models.OrganicResult>>();
        Assert.NotNull(search);
    }
}