# Repository Folder

This folder contains repository classes for the EzLeadGenerator web application. Repository classes provide data access and persistence logic for company profiles and job summaries, supporting both Azure Table Storage and local file-based storage.

---

## Table of Contents

- [Overview](#overview)
- [Repository Classes](#repository-classes)
  - [AzureCompanyRepository](#azurecompanyrepository)
  - [AzureJobsRepository](#azurejobsrepository)
  - [LocalCompanyProfileStore](#localcompanyprofilestore)
  - [LocalJobsRepositoryStore](#localjobsrepositorystore)
- [Usage](#usage)
- [Contact](#contact)

---

## Overview

The `Repository` folder provides implementations of the `ICompanyRepository` and `IJobsRepository` interfaces. These classes abstract the details of data storage and retrieval, allowing the application to work with either Azure Table Storage or local JSON files for development and production scenarios.

---

## Repository Classes

### AzureCompanyRepository

- **Purpose:**  
  Provides an implementation of `ICompanyRepository` using Azure Table Storage for persistent storage of company profiles.
- **Key Methods:**  
  - `GetCompanyProfileAsync(string companyId)` — Retrieve a company profile by ID.
  - `GetCompanyProfileAsync(DateTime fromDate)` — Retrieve company profiles updated since a given date.
  - `AddCompanyProfileAsync(CompanyProfile profile)` — Add a new company profile.
  - `UpdateCompanyProfileAsync(CompanyProfile profile)` — Update an existing company profile.
  - `DeleteCompanyProfileAsync(CompanyProfile profile)` — Delete a company profile.
- **Dependencies:**  
  - `TableClient`, `ICacheService`, `IOptions<AzureSettings>`, `ILogger<AzureCompanyRepository>`

---

### AzureJobsRepository

- **Purpose:**  
  Provides an implementation of `IJobsRepository` using Azure Table Storage for persistent storage of job summaries.
- **Key Methods:**  
  - `GetJobsAsync(string jobId)` — Retrieve a job summary by ID.
  - `GetJobsAsync(DateTime fromDate)` — Retrieve job summaries posted since a given date.
  - `AddJobAsync(JobSummary job)` — Add a new job summary.
  - `UpdateJobAsync(JobSummary job)` — Update an existing job summary.
  - `DeleteJobAsync(JobSummary job)` — Delete a job summary.
- **Dependencies:**  
  - `TableClient`, `ICacheService`, `IOptions<AzureSettings>`, `ILogger<AzureJobsRepository>`

---

### LocalCompanyProfileStore

- **Purpose:**  
  Provides a local file-based implementation of `ICompanyRepository` for managing company profiles in JSON files (for development or testing).
- **Key Methods:**  
  - `GetCompanyProfileAsync(string companyId)` — Retrieve a company profile by ID.
  - `GetCompanyProfileAsync(DateTime fromDate)` — Retrieve company profiles created since a given date.
  - `AddCompanyProfileAsync(CompanyProfile profile)` — Add a new company profile.
  - `UpdateCompanyProfileAsync(CompanyProfile profile)` — Update an existing company profile.
  - `DeleteCompanyProfileAsync(CompanyProfile profile)` — Delete a company profile.
- **Dependencies:**  
  - `ICacheService`, `IOptions<SerpApiSettings>`, `ILogger<LocalCompanyProfileStore>`

---

### LocalJobsRepositoryStore

- **Purpose:**  
  Provides a local file-based implementation of `IJobsRepository` for managing job summaries in JSON files (for development or testing).
- **Key Methods:**  
  - `GetJobsAsync(string jobId)` — Retrieve a job summary by ID.
  - `GetJobsAsync(DateTime fromDate)` — Retrieve job summaries posted since a given date.
  - `AddJobAsync(JobSummary job)` — Add a new job summary.
  - `UpdateJobAsync(JobSummary job)` — Update an existing job summary.
  - `DeleteJobAsync(JobSummary job)` — Delete a job summary.
- **Dependencies:**  
  - `ICacheService`, `IOptions<SerpApiSettings>`, `ILogger<LocalJobsRepositoryStore>`

---

## Usage

Choose the appropriate repository implementation based on your environment (Azure for production, local for development). Register the repository in your dependency injection container and use it through the `ICompanyRepository` or `IJobsRepository` interfaces.

Example:

```csharp
// Register in Startup.cs or Program.cs
services.AddCompanyProfileStore(Configuration)
        .AddJobsProfileStore(Configuration);
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