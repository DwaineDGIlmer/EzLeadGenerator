# Services Folder

This folder contains the core service classes for the EzLeadGenerator application layer. These services provide business logic for searching, retrieving, processing, and displaying job and company data, as well as integrating with external APIs and AI services.

---

## Table of Contents

- [Overview](#overview)
- [Service Classes](#service-classes)
  - [SerpApiSearchService](#serpapisearchservice)
  - [SerpApiSearchJobsService](#serpapisearchjobsservice)
  - [SearpApiSourceService](#searpapisourceservice)
  - [DisplayRepository](#displayrepository)
- [Usage](#usage)
- [Extending Services](#extending-services)
- [Contact](#contact)

---

## Overview

The `Services` folder provides implementations for searching job listings, retrieving company and job data, integrating with AI for organizational inference, and supporting data display and pagination. These services are designed for dependency injection and are consumed by higher-level application components.

---

## Service Classes

### SerpApiSearchService

- **Purpose:**  
  Performs Google search operations using the Serp API, handles caching of results, and logs operations.
- **Key Methods:**  
  - `FetchOrganicResults(query, location)` — Retrieves organic search results for a given query and location, with caching.
- **Dependencies:**  
  - `ICacheService`, `IHttpClientFactory`, `ILogger`, configuration via `SerpApiSettings`.

---

### SerpApiSearchJobsService

- **Purpose:**  
  Retrieves job listings from the Serp API (Google Jobs), supports pagination, and caches results.
- **Key Methods:**  
  - `FetchJobs(query, location)` — Fetches job listings for a given query and location, handling pagination and caching.
- **Dependencies:**  
  - `ICacheService`, `IHttpClientFactory`, `ILogger`, configuration via `SerpApiSettings`.

---

### SearpApiSourceService

- **Purpose:**  
  Integrates search, AI, and repository services to update job sources and company profiles. Uses AI to infer organizational hierarchies and job divisions.
- **Key Methods:**  
  - `UpdateCompanyProfilesAsync()` — Updates or adds company profiles based on recent job data and AI-inferred hierarchies.
  - `UpdateJobSourceAsync()` — Fetches and validates jobs, infers divisions, and adds them to the repository.
- **Dependencies:**  
  - `ICacheService`, `ISearch<OrganicResult>`, `IJobsRetrieval<JobResult>`, `IOpenAiChatService`, `ICompanyRepository`, `IJobsRepository`, `ILogger`, configuration via `SerpApiSettings`.
- **Related Models:**  
  - `HierarchyResults`, `HierarchyItem` (defined within the same file for organizational hierarchy data).

---

### DisplayRepository

- **Purpose:**  
  Provides paginated access to job summaries and company profiles for display purposes.
- **Key Methods:**  
  - `GetPaginatedJobsAsync(fromDate, page, pageSize)` — Retrieves a page of job summaries.
  - `GetPaginatedCompaniesAsync(fromDate, page, pageSize)` — Retrieves a page of company profiles.
  - `GetJobCount(fromDate)` / `GetCompanyCount(fromDate)` — Returns counts for jobs and companies since a given date.
- **Dependencies:**  
  - `ICompanyRepository`, `IJobsRepository`, `ILogger`.

---

## Usage

These services are registered for dependency injection and are used by controllers, background jobs, or other application components to perform business logic, data retrieval, and integration with external APIs.

Example (in a controller or background service):

```csharp
var jobs = await _serpApiSearchJobsService.FetchJobs("Data Engineer", "Charlotte, NC");
var companies = await _displayRepository.GetPaginatedCompaniesAsync(DateTime.Now.AddDays(-30), 1, 10);
```

---

## Extending Services

To add new services or extend existing ones:

1. Create a new class in this folder, implementing the appropriate interface(s).
2. Register the service in your application's dependency injection container.
3. Update documentation and usage examples as needed.

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