# EzLeadGenerator

EzLeadGenerator is an AI-assisted .NET web application designed to automate and streamline the process of generating thousands of high-quality leads.

## Features

- AI-powered lead generation
- Search and filter job postings by title and location
- Grouped job results for easy analysis
- Modern ASP.NET Core web application
- Configurable via `appsettings.json`
- Unit tested for reliability

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022+ or Visual Studio Code

### Build and Run

1. Clone the repository:
    ```sh
    git clone https://github.com/DwaineDGIlmer/EzLeadGenerator.git
    cd EzLeadGenerator
    ```

2. Restore dependencies:
    ```sh
    dotnet restore
    ```

3. Build the project:
    ```sh
    dotnet build
    ```

4. Run the web application:
    ```sh
    dotnet run --project src/WebApp
    ```

5. Open your browser and navigate to `https://localhost:5001` (or the URL shown in the console).

### Configuration

Edit `src/WebApp/appsettings.json` to set your API keys and endpoints:
```json
{
  "SearchApiKey": "your-api-key",
  "SearchJobsEndpoint": "https://your-api-endpoint"
}
```

## Testing

Run unit tests with:
```sh
dotnet test
```

## License

This project is licensed under the MIT License.  
Copyright © 2025 Dwain Gilmer

## Author

[Dwain Gilmer](mailto:dwaine.gilmer@protonmail.com)

## Links

- [Project Homepage](https://github.com/DwaineDGIlmer/EzLeadGenerator)