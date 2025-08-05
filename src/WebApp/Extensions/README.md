# Extensions Folder

This folder contains extension classes that provide utility methods for caching, logging, and service registration in the EzLeadGenerator web application.

---

## Table of Contents

- [Overview](#overview)
- [Extension Classes](#extension-classes)
  - [Extensions](#extensions)
  - [ServiceCollectionExtensions](#servicecollectionextensions)
- [Usage](#usage)
- [Contact](#contact)

---

## Overview

The `Extensions` folder provides reusable extension methods for cache operations, dependency injection, and configuration binding. These help simplify common tasks and promote code reuse throughout the application.

---

## Extension Classes

### Extensions

- **Purpose:**  
  Provides extension methods for cache operations and logging related to job summaries and company profiles.
- **Key Methods:**  
  - `GetJobAsync` — Retrieve a cached job summary by job ID.
  - `GetJobsAsync` — Retrieve a list of cached job summaries by date.
  - `AddJobsAsync` — Add job summaries to the cache.
  - `GetCompanyAsync` — Retrieve a cached company profile by company ID.
  - `GetCompaniesAsync` — Retrieve a list of cached company profiles by date.
  - `AddCompaniesAsync` — Add company profiles to the cache.
  - `GetCacheKey` — Generate a cache key using a prefix and date.
- **Dependencies:**  
  - `ICacheService`, `ILogger`, `JobSummary`, `CompanyProfile`

---

### ServiceCollectionExtensions

- **Purpose:**  
  Provides extension methods for registering services and configuration with the dependency injection container.
- **Key Methods:**  
  - `AddCompanyProfileStore` — Register company profile repository based on environment.
  - `AddJobsProfileStore` — Register jobs repository based on environment.
  - `ConfigureSerpApiSettings` — Bind and configure SerpApi settings.
  - `AddJobsRetrivalService` — Register jobs retrieval service and HTTP client.
  - `AddCacheService` — Register cache service (in-memory or file system).
  - `AddDisplayRepository` — Register display repository service.
  - `AddJobSourceService` — Register job source and AI chat services.
  - `AddSearchService` — Register search service.
- **Dependencies:**  
  - `IServiceCollection`, `IConfiguration`, `ILogger`, `ICacheService`, `ICompanyRepository`, `IJobsRepository`, `SerpApiSettings`, `AzureSettings`, `IHttpClientFactory`, `IDisplayRepository`, `IJobSourceService`, `IAiChatService`, `IOpenAiChatService`

---

## Usage

Use these extension methods to simplify cache access and service registration in your application startup and business logic.

Example:

```csharp
// Register services in Program.cs or Startup.cs
services.AddCompanyProfileStore(Configuration)
        .AddJobsProfileStore(Configuration)
        .AddCacheService(Configuration)
        .AddDisplayRepository();
```

---

## Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements and bug fixes.  
See [CONTRIBUTING.md](../../CONTRIBUTING.md) for guidelines.

---

## License

This project is licensed under the [MIT License](../../LICENSE).

---

## Contact

For questions or support, please contact Dwaine Gilmer at [Protonmail.com](mailto:dwaine.gilmer@protonmail.com) or submit an issue on the project's GitHub