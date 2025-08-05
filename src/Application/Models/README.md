# Application.Models

This folder contains the core data models used throughout the EzLeadGenerator application. These models represent business entities, search and job results, and utility types for serialization and inference.

---

## Table of Contents

- [Overview](#overview)
- [Model Classes](#model-classes)
  - [CompanyProfile](#companyprofile)
  - [JobSummary](#jobsummary)
  - [GoogleSearchResult](#googlesearchresult)
  - [GoogleJobsResult](#googlejobsresult)
  - [DivisionInference](#divisioninference)
  - [DateTimeConverter](#datetimeconverter)
  - [ClientResultJsonConverter\<T\>](#clientresultjsonconvertert)
- [Usage Example](#usage-example)
- [Related Files](#related-files)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## Overview

The `Models` folder provides the main data structures for company profiles, job summaries, search results, and utility types. These models are used for data transfer, storage, and business logic throughout the application.

---

## Model Classes

### CompanyProfile

- **Purpose:**  
  Represents a company's profile, including metadata, organizational structure, and analysis for job and department inference.
- **Key Properties:**  
  - Company name, metadata, divisions, AI analysis.
- **Related Models:**  
  - `DivisionInference`

---

### JobSummary

- **Purpose:**  
  Represents a summary of a job posting, including company details, job title, location, description, and AI analysis.
- **Key Properties:**  
  - Job title, company, location, description, analysis.

---

### GoogleSearchResult

- **Purpose:**  
  Represents the search result data retrieved from a search engine.
- **Key Properties:**  
  - Metadata, parameters, organic results, related searches, pagination.

---

### GoogleJobsResult

- **Purpose:**  
  Represents the results of a Google Jobs search.
- **Key Properties:**  
  - Metadata, parameters, filters, job listings, pagination.

---

### DivisionInference

- **Purpose:**  
  Represents an inference about a division, including the reasoning and confidence level.
- **Key Properties:**  
  - Division name, reasoning, confidence.

---

### DateTimeConverter

- **Purpose:**  
  Custom JSON converter for serializing and deserializing `DateTime` objects in ISO 8601 format.
- **Key Methods:**  
  - Serialization and deserialization logic for `DateTime`.

---

### ClientResultJsonConverter&lt;T&gt;

- **Purpose:**  
  Custom JSON converter for serializing and deserializing `ClientResult<T>` objects, handling encapsulation within a "value" property.
- **Key Methods:**  
  - Serialization and deserialization logic for `ClientResult<T>`.

---

## Usage Example

These models are used for data transfer, storage, and business logic throughout the application.

```csharp
// Creating a new job summary from a job result
var jobSummary = new JobSummary(jobResult);

// Serializing a company profile to JSON
var json = JsonSerializer.Serialize(companyProfile);

// Using the DateTimeConverter for custom serialization
var options = new JsonSerializerOptions();
options.Converters.Add(new DateTimeConverter());
var dateJson = JsonSerializer.Serialize(DateTime.Now, options);
```

---

## Related Files

- [CompanyProfile.cs](CompanyProfile.cs)
- [JobSummary.cs](JobSummary.cs)
- [GoogleSearchResult.cs](GoogleSearchResult.cs)
- [GoogleJobsResult.cs](GoogleJobsResult.cs)
- [DivisionInference.cs](DivisionInference.cs)

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