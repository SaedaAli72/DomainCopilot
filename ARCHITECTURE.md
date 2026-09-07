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


## Entity-Relationship Diagram

```mermaid
erDiagram
    TENANT ||--o{ CITIZEN_REQUEST : "has many"
    TENANT ||--o{ DOCUMENT : "has many"
    DOCUMENT ||--o{ DOCUMENT_CHUNK : "has many"

    TENANT {
        guid Id PK
        string Name
        bool IsActive
        datetime CreatedAt
    }

    CITIZEN_REQUEST {
        guid Id PK
        guid TenantId FK
        string CitizenName
        string ServiceType
        string Status
        string DraftedResponseText
        datetime CreatedAt
    }

    DOCUMENT {
        guid Id PK
        guid TenantId FK
        string FileName
        string Version
        string Status
        datetime UploadedAt
    }

    DOCUMENT_CHUNK {
        guid Id PK
        guid DocumentId FK
        string Content
        int PageNumber
        string SectionReference
        bytes Embedding
    }
```


## Sequence Diagram — Full Request Flow

```mermaid
sequenceDiagram
    participant Officer as Government Officer
    participant API as ProcessRequestController
    participant Orch as Orchestrator
    participant Elig as EligibilityAgent
    participant Proc as ProcedureAgent
    participant Draft as ResponseDrafterAgent
    participant DB as SQL Server

    Officer->>API: POST /ProcessRequest
    API->>DB: Save CitizenRequest (PendingReview)
    API->>Orch: ProcessAsync(situation, serviceType)

    Orch->>Elig: AnalyzeAsync(situation)
    Elig->>DB: Retrieve chunks (RAG)
    Elig-->>Orch: EligibilityResult

    alt Requires Escalation
        Orch-->>API: null
        API->>DB: Update status = Escalated
        API-->>Officer: "Escalated - human review needed"
    else Eligible/Not Eligible
        Orch->>Proc: ResolveAsync(serviceType)
        Proc->>DB: Retrieve chunks (RAG)
        Proc-->>Orch: ProcedureResult

        Orch->>Draft: DraftAsync(eligibility, procedure)
        Draft-->>Orch: DraftedResponse

        Orch-->>API: DraftedResponse
        API->>DB: Update status = AwaitingApproval
        API-->>Officer: requestId + draft (pending approval)

        Officer->>API: POST /Approval/{id}/approve
        API->>DB: Update status = Approved
        API-->>Officer: Approved response text
    end
```