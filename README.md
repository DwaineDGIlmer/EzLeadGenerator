# EzLeadGenerator

EzLeadGenerator is an AI-assisted .NET web application designed to automate and streamline the process of generating thousands of high-quality leads for recruiting and staffing professionals.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

---

## Features

- **AI-powered lead generation:** Analyze and enrich job and company data using AI.
- **Advanced search:** Filter job postings by title and location.
- **Grouped results:** Easily analyze jobs by company or division.
- **Modern ASP.NET Core web application:** Built with .NET 8.
- **Configurable:** All settings via `appsettings.json` or environment variables.
- **Unit tested:** Reliable and maintainable codebase.

---

## Table of Contents

- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)
- [Author](#author)
- [Links](#project-links)

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ or Visual Studio Code

### Build and Run

1. **Clone the repository:**
    ```sh
    git clone https://github.com/DwaineDGIlmer/EzLeadGenerator.git
    cd EzLeadGenerator
    ```

2. **Restore dependencies:**
    ```sh
    dotnet restore
    ```

3. **Build the project:**
    ```sh
    dotnet build
    ```

4. **Run the web application:**
    ```sh
    dotnet run --project src/WebApp
    ```

5. **Open your browser and navigate to:**  
   [https://localhost:5001](https://localhost:5001) (or the URL shown in the console).

---

## Configuration

Edit [`src/WebApp/appsettings.json`](src/WebApp/appsettings.json) to set your API keys and endpoints:

```json
{
  "SearchApiKey": "your-api-key",
  "SearchJobsEndpoint": "https://your-api-endpoint"
}
```

For secure local development, use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```sh
dotnet user-secrets set "SearchApiKey" "<your-api-key>"
```

---

## Testing

Run unit tests with:
```sh
dotnet test
```

---

## Project Structure

```
EzLeadGenerator/
├── src/
│   ├── Application/   # Core business logic, models, contracts, and services
│   └── WebApp/        # ASP.NET Core web application
└── README.md          # This file
```

- [`src/Application/README.md`](src/Application/README.md)  
  Details on the application layer: models, contracts, services, and configuration.
- [`src/WebApp/README.md`](src/WebApp/README.md)  
  Details on the web application: extensions, middleware, repositories, and structure.

---

## Contributing

Contributions are welcome! Please open an issue or pull request.

---

## License

This project is licensed under the [MIT License](LICENSE).  
Copyright © 2025 Dwain Gilmer

---

## Author

[Dwain Gilmer](mailto:dwaine.gilmer@protonmail.com)

---

## Project Links

- [Project Homepage](https://github.com/DwaineDGIlmer/EzLeadGenerator)
- [Application Layer README](./src/Application/README.md)
- [Web Application README](./src/WebApp/README.md)
