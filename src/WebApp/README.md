# EzLeadGenerator WebApp

EzLeadGenerator WebApp is a modern ASP.NET Core application that automates lead generation and analysis for recruiting and staffing professionals.

## Features

- AI-powered job and company profile enrichment
- Search and filter job postings by title and location
- Grouped job results for easy analysis
- Paginated job listings and CSV export
- Company analysis with organizational hierarchy
- Privacy-first demo (no user data stored)
- Configurable via `appsettings.json`
- Supports both local file and Azure Table/Blob storage

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ or Visual Studio Code

### Build and Run

1. Restore dependencies:
    ```sh
    dotnet restore
    ```
2. Build the project:
    ```sh
    dotnet build
    ```
3. Run the web application:
    ```sh
    dotnet run --project src/WebApp
    ```
4. Open your browser and navigate to `https://localhost:5001` (or the URL shown in the console).

### Configuration

Edit [`src/WebApp/appsettings.json`](src/WebApp/appsettings.json) to set your API keys and endpoints.

## Testing

Run unit tests with:
```sh
dotnet test
```

## License

This project is licensed under the MIT License.

## Author

[Dwain Gilmer](mailto:dwaine.gilmer@protonmail.com)

## Links

- [Project Homepage](https://github.com/DwaineDGIlmer/EzLeadGenerator)

---

## Secure Local Development

**Never commit secrets or connection strings to GitHub.**  
Follow these steps to test locally without violating security:

### 1. Use User Secrets (Recommended for ASP.NET Core)

Store your Azure Table Storage connection string securely for local development:

```sh
dotnet user-secrets set "ConnectionStrings:AzureTableStorage" "<your-local-connection-string>"
```

This keeps secrets out of `appsettings.json` and source control.

### 2. Use Placeholders in `appsettings.json`

Your `appsettings.json` should use an empty value or placeholder:

```json
"ConnectionStrings": {
  "AzureTableStorage": ""
}
```

### 3. Never Commit Secrets

Do not commit real connection strings or secrets to GitHub.  
If you use files like `.env` or `secrets.json`, add them to `.gitignore`:

```
secrets.json
.env
```

### 4. Use Environment Variables in Production

For Azure deployments, set connection strings in the Azure Portal under  
`resourceGroups/AiEventing/providers/Microsoft.Web/sites/EzLeadGenerator/connectionStrings`.

### 5. Document Setup

Add these instructions to your `README.md` so all contributors know how to set up secrets securely.

---

**For questions or contributions, please open an issue or pull request.**