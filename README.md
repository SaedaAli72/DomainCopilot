# Domain Copilot

An Agentic RAG platform for government citizen services (D4), built with 
Clean Architecture in ASP.NET Core 8, featuring multi-tenant isolation (T0), 
a 3-agent workflow with human approval gate, and real-time streaming.

## Prerequisites

- .NET 8 SDK
- SQL Server (Express or full) with a local instance
- A free Google Gemini API key (see below)

## Quick Start

1. Clone the repository
2. Set your Gemini API key:
```powershell
   dotnet user-secrets set "Gemini:ApiKey" "YOUR_KEY_HERE" --project DomainCopilot.Api
```
3. Update the connection string in `appsettings.json` if your SQL Server instance 
   name differs from the default
4. Apply migrations:
```powershell
   dotnet ef database update --project DomainCopilot.Infrastructure --startup-project DomainCopilot.Api
```
5. Run the API:
```powershell
   dotnet run --project DomainCopilot.Api
```
6. Open Swagger at `https://localhost:[port]/swagger`