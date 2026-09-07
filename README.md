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



## Environment Variables

| Variable | Description | Required |
|---|---|---|
| `Gemini:ApiKey` | Google Gemini API key (set via User Secrets, not in appsettings.json) | Yes |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string (in `appsettings.json`) | Yes |

## Getting a Free API Key

1. Go to [https://aistudio.google.com/apikey](https://aistudio.google.com/apikey)
2. Sign in with a Google account
3. Click "Create API Key"
4. Set it via User Secrets (never commit it to source control):
```powershell
   dotnet user-secrets set "Gemini:ApiKey" "YOUR_KEY_HERE" --project DomainCopilot.Api
```

## Running Tests

```powershell
dotnet test
```

## Running the Evaluation Harness

```powershell
dotnet test --filter "FullyQualifiedName~EvaluationHarnessTests"
```

**Note**: The Gemini free tier has strict rate limits (observed: ~20 requests per 
rolling window). Running all 25 evaluation questions may hit rate limits — see 
`docs/EVALUATION.md` for documented results and this known limitation.


## 5-Minute Demo Path

Follow these steps in order to see every core capability:

1. **Start the API** (`dotnet run --project DomainCopilot.Api`) and open Swagger 
   at `https://localhost:[port]/swagger`

2. **Ingest a document** — `POST /api/Ingest`:
```json
   {
     "fileName": "Housing_Policy.pdf",
     "rawText": "الحد الأقصى للدخل الشهري المسموح به للأسرة هو 8000 جنيه مصري."
   }
```
   → Confirms document chunking + real Gemini embedding + SQL vector indexing.

3. **Ask a grounded question** — `POST /api/Ask`:
```json
   "كام الحد الأقصى للدخل؟"
```
   → Returns an answer citing the ingested content (Groundedness).

4. **Run the full agent workflow** — `POST /api/ProcessRequest`:
```json
   {
     "citizenName": "أحمد محمد",
     "citizenSituation": "المواطن دخله الشهري 6000 جنيه",
     "serviceType": "الدعم السكني"
   }
```
   → Runs all 3 agents (Eligibility → Procedure → Response Drafter); returns a 
   `requestId` with status `awaiting_approval` (Human Approval Gate).

5. **View pending approvals** — `GET /api/Approval/pending`
   → Shows the request from step 4, awaiting officer review.

6. **Approve the request** — `POST /api/Approval/{requestId}/approve`
   → Finalizes the response; demonstrates Human-in-the-Loop control.

7. **See live streaming** — open in a browser (not Swagger):

https://localhost:[port]/api/ProcessRequestStream/stream?citizenSituation=...&serviceType=...

   → Shows real-time agent progress events (SSE).

8. **Verify tenant isolation** — run:
```powershell
   dotnet test --filter "FullyQualifiedName~TenantIsolationTests"
```
   → Automated proof that cross-tenant data leakage is impossible (T0).