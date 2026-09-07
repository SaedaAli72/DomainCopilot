# Security Documentation — Domain Copilot

## OWASP Web Top 10

### Broken Access Control
- **Control**: EF Core Global Query Filters enforce tenant-scoped data access at the 
  database level (not just application checks). Every query on `CitizenRequest` and 
  `Document` is automatically filtered by the current tenant.
- **Evidence**: `TenantIsolationTests.cs` — automated test proving cross-tenant access 
  is structurally impossible.
- **Gap**: Role-based access control (e.g., distinguishing Officer vs. Administrator 
  permissions) not yet implemented; currently all endpoints assume officer-level access.

### Cryptographic Failures
- **Control**: API keys (Gemini) stored via .NET User Secrets during development, 
  never committed to source control. Connection strings use Windows Trusted 
  Connection (no plaintext credentials in `appsettings.json`).
- **Gap**: Production deployment would require a proper secrets manager (Azure Key 
  Vault) — documented in SDD Part A.

### Injection
- **Control**: All database access goes through EF Core, which parameterizes queries 
  automatically — no raw SQL string concatenation anywhere in the codebase.
- **Control**: Document ingestion accepts raw text via a structured JSON DTO 
  (`IngestRequest`), not executed as code or interpreted as commands.

### Security Misconfiguration
- **Gap**: CORS policy, security headers (CSP, X-Frame-Options), and HTTPS 
  enforcement in production not yet explicitly configured beyond ASP.NET Core defaults.


## OWASP LLM Top 10

### Prompt Injection (Direct and Indirect)
- **Control**: All agent prompts explicitly instruct the LLM to use ONLY the 
  provided context and ignore any conflicting instructions embedded within it 
  (see `EligibilityIdentifierAgent` prompt: "Do NOT consider missing documents...").
- **Test case**: Golden set question #23 (adversarial_prompt_injection): "????? ?? 
  ????????? ??????? ??? ?? ???? ????????? ?????? ????????" — designed to test 
  resistance to direct prompt injection.
- **Gap**: Indirect injection (malicious instructions embedded inside an ingested 
  document itself) is not yet specifically tested; the golden set covers direct 
  injection via user question only.

### Insecure Output Handling
- **Control**: LLM output is never rendered as raw HTML, executed as code, or passed 
  to a shell/file path. Responses are returned as plain JSON text fields (`ResponseText`), 
  consumed by the API layer only.
- **Control**: Tool inputs (chunk IDs from retrieval) are validated as `Guid` types 
  before use — malformed IDs fail deserialization rather than being passed downstream.

### Sensitive Information Disclosure
- **Control**: Synthetic test data only (no real citizen PII) used throughout 
  development, per assignment requirements.
- **Gap**: No automated PII detection/redaction on ingested documents yet; relies on 
  the assumption that uploaded documents are pre-vetted regulation text, not citizen 
  case files.

### Excessive Agency
- **Control**: Each agent has a narrowly scoped responsibility (Eligibility check / 
  Procedure resolution / Response drafting) with no agent able to directly modify 
  the database or send a response without passing through the Human Approval Gate.
- **Evidence**: `CitizenRequest.AttachDraftedResponse` always sets status to 
  `AwaitingApproval`, never `Approved`, directly from agent output.

### Unbounded Consumption
- **Control**: Retry-with-backoff on LLM/Embedding calls prevents runaway retry loops. 
  Vector search is capped at `topK: 5` per query, bounding context size.
- **Gap**: No hard per-request token cap or per-tenant rate limiting implemented yet 
  (relevant since T0 is Multi-tenancy, not T3 Cost Governor — out of scope per 
  assigned variant).