# Architecture Documentation — Domain Copilot

## Layer Dependency Diagram

```mermaid
graph TD
    Domain[Domain Layer<br/>Entities, Enums]
    Application[Application Layer<br/>Interfaces, UseCases, Agents]
    Infrastructure[Infrastructure Layer<br/>EF Core, Gemini, SQL Vector Store]
    Api[Api Layer<br/>Controllers, Program.cs]

    Application --> Domain
    Infrastructure --> Application
    Api --> Application
    Api --> Infrastructure
```

This confirms the Clean Architecture boundary: `Domain` has zero outgoing 
dependencies, and all other layers depend inward toward it, never outward.