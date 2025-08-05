# 🧠 EzLeadGenerator Application Layer

The Application layer contains the core business logic, models, contracts, and service interfaces for the EzLeadGenerator solution. It is designed for use by the WebApp and supporting infrastructure projects.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../../LICENSE)

---

## ✨ Features

- **Domain models:** Jobs, companies, summaries, and more.
- **Service and repository contracts:** For dependency injection and testability.
- **Configuration objects:** For external services (Azure, SerpApi, OpenAI, etc.).
- **Extension methods:** Helpers for common tasks and utilities.

---

## 📚 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Project Structure](#project-structure)
- [Folder References](#folder-references)
- [Usage](#usage)
- [Installation](#installation)
- [Links](#project-links)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## 🚀 Overview

This project contains the core business logic, models, contracts, and service interfaces for the EzLeadGenerator solution. It is designed to be consumed by both the WebApp and supporting infrastructure projects.

---

## 🗂️ Project Structure

```
src/
└── Application/
    ├── Configurations/   # Configuration classes for external services and settings
    ├── Contracts/        # Interfaces for repositories, services, and business logic
    ├── Models/           # Core data models (jobs, companies, etc.)
    ├── Services/         # Service implementations for business logic
    ├── Extensions/       # Extension methods and helpers
    └── README.md         # This file
```

---

### Folder References

- [Models README](./Models/README.md) — Core data models for jobs, companies, search results, and utility types.
- [Contracts README](./Contracts/README.md) — Interfaces for repositories, services, and business logic contracts.
- [Configurations README](./Configurations/README.md) — Strongly-typed configuration classes for external services and application settings.
- [Services README](./Services/README.md) — Service implementations for search, job retrieval, and data display.
- [Extensions README](./Extensions/README.md) — Extension methods and helpers for the application layer.

---

## 🚀 Usage

Reference this project from your ASP.NET Core WebApp or any other .NET project that needs access to the core business logic and contracts.

---

## 💾 Installation

1. **Clone the repository:**
   ```sh
   git clone https://github.com/yourusername/EzLeadGenerator.git
   ```
2. **Add a project reference to `src/Application` in your .NET solution:**
   ```sh
   dotnet add reference ../Application/Application.csproj
   ```
3. **Restore dependencies and build:**
   ```sh
   dotnet restore
   dotnet build
   ```

---

## 🔗 Project Links

- [Project Homepage](https://github.com/DwaineDGIlmer/EzLeadGenerator)
- [Configurations README](./Configurations/README.md)
- [Contracts README](./Contracts/README.md)
- [Models README](./Models/README.md)
- [Services README](./Services/README.md)

---

## 🤝 Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements and bug fixes.  
See [CONTRIBUTING.md](../../CONTRIBUTING.md) for guidelines.

---

## 📄 License

This project is licensed under the [MIT License](../../LICENSE).

---

## 📬 Contact

For questions or support, please contact Dwaine Gilmer at [Protonmail.com](mailto:dwaine.gilmer@protonmail.com) or submit an issue on the project's GitHub
