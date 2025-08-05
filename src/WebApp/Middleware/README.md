# Middleware Folder

This folder contains middleware classes for the EzLeadGenerator web application. Middleware components handle cross-cutting concerns during HTTP request processing, such as periodic background updates.

---

## Table of Contents

- [Overview](#overview)
- [Middleware Classes](#middleware-classes)
  - [JobServicesMiddleware](#jobservicesmiddleware)
- [Usage](#usage)
- [Contact](#contact)

---

## Overview

The `Middleware` folder provides components that execute logic during the HTTP request pipeline. These are used to trigger background updates, logging, or other operations that should run alongside normal request handling.

---

## Middleware Classes

### JobServicesMiddleware

- **Purpose:**  
  Periodically updates job sources and company profiles during HTTP request processing, based on a configured interval.
- **Key Methods:**  
  - `InvokeAsync(HttpContext context)` — Checks if the update interval has elapsed and triggers updates if needed.
  - `UpdateSourceAsync()` — Calls services to update job sources and company profiles.
- **Dependencies:**  
  - `IJobSourceService`, `IOptions<EzLeadSettings>`, `ILogger<JobServicesMiddleware>`, `RequestDelegate`

---

## Usage

Register the middleware in your application's pipeline (e.g., in `Startup.cs` or `Program.cs`):

```csharp
app.UseMiddleware<JobServicesMiddleware>();
```

This ensures that job source and company profile updates are checked and performed as part of each HTTP request, according to the configured interval.

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