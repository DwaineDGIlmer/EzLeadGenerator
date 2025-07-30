# Application

EzLeadGenerator Application Layer

## Overview

This project contains the core business logic, models, contracts, and service interfaces for the EzLeadGenerator solution. It is designed to be consumed by both the WebApp and supporting infrastructure projects.

## Features

- Domain models for jobs, companies, and summaries
- Service and repository contracts for dependency injection
- Configuration objects for external services (Azure, SerpApi, OpenAI, etc.)
- Extension methods and helpers for common tasks

## Structure

- **Models**: Strongly-typed classes for jobs, companies, and related entities
- **Contracts**: Interfaces for repositories and services
- **Configurations**: Classes for binding configuration settings
- **Extensions**: Utility and helper methods

## Usage

Reference this project from your ASP.NET Core WebApp or any other .NET project that needs access to the core business logic and contracts.

## Contributing

Contributions are welcome! Please submit issues or pull requests via GitHub.

## License

MIT License