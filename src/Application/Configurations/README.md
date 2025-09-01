# Application.Configurations

This folder contains configuration classes for the EzLeadGenerator application. These classes define strongly-typed settings objects that are used to bind and access configuration values throughout the application.

---

## Table of Contents

- [Overview](#overview)
- [Configuration Classes](#configuration-classes)
  - [AzureSettings](#azuresettings)
  - [EzLeadSettings](#ezleadsettings)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## Overview

The `Configurations` folder provides settings classes for Azure, job execution, and other application-level configuration. These classes are designed for use with .NET's configuration system and dependency injection.

---

## Configuration Classes

### AzureSettings

- **Purpose:**  
  Holds configuration for Azure storage and caching.
- **Key Properties:**  
  - `AzureTableName`, `CacheExpirationInMinutes`, `CompanyProfilePartionKey`, `JobSummaryPartionKey`, `CompanyProfileTableName`, `JobSummaryTableName`
- **Usage:**  
  Bind to configuration section and inject via `IOptions<AzureSettings>`.

---

### EzLeadSettings

- **Purpose:**  
  Holds configuration for job execution timing.
- **Key Properties:**  
  - `JobExecutionInSeconds`
- **Usage:**  
  Bind to configuration section and inject via `IOptions<EzLeadSettings>`.

---

## Usage

These classes are typically bound to configuration sections in `appsettings.json` or environment variables using .NET's configuration system. Inject them via `IOptions<T>` or `IOptionsSnapshot<T>` where needed.

```csharp
public sealed class MyService
{
    private readonly AzureSettings _azureSettings;
    public MyService(IOptions<AzureSettings> azureOptions)
    {
        _azureSettings = azureOptions.Value;
    }
}
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