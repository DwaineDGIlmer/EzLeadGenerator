# 🧠 EzLeadGenerator WebApp

EzLeadGenerator WebApp is a modern ASP.NET Core application that automates lead generation and analysis for recruiting and staffing professionals.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## ✨ Features

- **AI-powered enrichment:** Job and company profile analysis using AI.
- **Advanced search:** Filter job postings by title and location.
- **Grouped results:** Easily analyze jobs by company or division.
- **Pagination & export:** Paginated job listings and CSV export.
- **Company insights:** Organizational hierarchy and division inference.
- **Privacy-first demo:** No user data stored.
- **Flexible storage:** Supports both local file and Azure Table/Blob storage.
- **Configurable:** All settings via `appsettings.json` or environment variables.

---

## 📚 Table of Contents

- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Secure Local Development](#secure-local-development)
- [Contributing](#contributing)
- [License](#license)
- [Author](#author)
- [Links](#project-links)

---

## 🛠️ Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ or Visual Studio Code

### Build and Run

1. **Restore dependencies:**
    ```sh
    dotnet restore
    ```
2. **Build the project:**
    ```sh
    dotnet build
    ```
3. **Run the web application:**
    ```sh
    dotnet run --project src/WebApp
    ```
4. **Browse to:**  
   [https://localhost:5001](https://localhost:5001) (or the URL shown in the console)

---

## ⚙️ Configuration

Edit [`src/WebApp/appsettings.json`](src/WebApp/appsettings.json) to set your API keys and endpoints.

- **Connection Strings:**  
  Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development:
  ```sh
  dotnet user-secrets set "ConnectionStrings:AzureTableStorage" "<your-local-connection-string>"
  ```
- **Production:**  
  Set connection strings in the Azure Portal under  
  `resourceGroups/AiEventing/providers/Microsoft.Web/sites/EzLeadGenerator/connectionStrings`.

---

## 🧪 Testing

Run unit tests with:
```sh
dotnet test
```

---

## 🗂️ Project Structure
```
src/
└── WebApp/
    ├── Extensions/       # Extension methods and helpers
    ├── Middleware/       # Custom middleware for request handling and logging
    ├── Pages/            # Razor pages for the web application
    ├── Repository/       # Repository implementations for data access and persistence
    └── README.md         # This file
```
<details>
  <summary>Expand for details</summary>
  The solution is organized into several libraries and folders, most with its own documentation:

- [WebApp](.)

  Web application project that consumes the Application layer and provides a user interface for lead generation.
  - [Extensions](./Extensions/README.md) — Contains extension methods and helpers for the application layer.
  - [Middleware](./Middleware/README.md) — Custom middleware for request handling and logging.
  - [Repository](./Repository/README.md) — Contains repository implementations for data access and persistence.

  - [WebApp.UnitTests](tests/WebApp.UnitTests)
  Unit tests for the the Web Application.
</details>

---

## 🔒 Secure Local Development

**Never commit secrets or connection strings to GitHub.**  
Follow these steps to test locally without violating security:

1. **Use User Secrets (Recommended for ASP.NET Core)**
    ```sh
    dotnet user-secrets set "ConnectionStrings:AzureTableStorage" "<your-local-connection-string>"
    ```
2. **Use Placeholders in `appsettings.json`**
    ```json
    "ConnectionStrings": {
      "AzureTableStorage": ""
    }
    ```
3. **Never Commit Secrets**
    - Add files like `.env` or `secrets.json` to `.gitignore`:
      ```
      secrets.json
      .env
      ```
4. **Use Environment Variables in Production**
    - Set connection strings in the Azure Portal as described above.
5. **Document Setup**
    - Add these instructions to your `README.md` so all contributors know how to set up secrets securely.

---

## 🤝 Contributing

Contributions are welcome! Please open an issue or pull request.

---

## 📄 License

This project is licensed under the [MIT License](../LICENSE).

---

## 🧑‍💻 Author

[Dwain Gilmer](mailto:dwaine.gilmer@protonmail.com)
[GitHub](https://github.com/DwaineDGIlmer)  

---

## 🔗 Project Links

- [Project Homepage](https://github.com/DwaineDGIlmer/EzLeadGenerator)
- [Extensions README](./Extensions/README.md)
- [Middleware README](./Middleware/README.md)
- [Repository README](./Respository/README.md)
