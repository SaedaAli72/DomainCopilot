## 1. Context

Government agencies handling citizen service requests (e.g., housing subsidies) 
currently rely on manual review of regulations and eligibility criteria, which is 
slow, error-prone, and difficult to audit. Domain Copilot addresses this by 
providing an AI-assisted, evidence-grounded system that helps government officers 
process citizen requests faster while maintaining human oversight over all 
consequential decisions.

## 2. Personas

| Persona | Description | Goals |
|---|---|---|
| **Government Officer** | Reviews citizen requests, approves/rejects AI-drafted responses | Fast, accurate processing with full traceability |
| **Citizen** (indirect) | Submits a service request/situation | Receive accurate, timely response |
| **System Administrator** | Manages tenant onboarding, document uploads | Ensure data isolation and system health |

## 3. Objectives

| ID | Objective | Success Metric |
|---|---|---|
| OBJ-01 | Provide grounded answers to citizen service inquiries | Retrieval hit-rate ? 80% on golden set |
| OBJ-02 | Prevent hallucinated eligibility/entitlement claims | Refusal correctness ? 90% on out-of-corpus questions |
| OBJ-03 | Ensure no response reaches a citizen without human approval | 100% of drafted responses pass through Approval Gate |
| OBJ-04 | Guarantee complete data isolation between government tenants | 0 cross-tenant data leaks (proven by automated test) |

## 4. Requirements

| ID | Requirement | Acceptance Criteria |
|---|---|---|
| BR-01 | System must ingest government policy documents (PDF/text) | Document uploaded, chunked, embedded, and indexed successfully |
| BR-02 | System must answer citizen questions with cited sources | Every answer references specific document chunks |
| BR-03 | System must refuse to answer when evidence is insufficient | Response contains "Not enough information" for out-of-corpus questions |
| BR-04 | System must identify citizen eligibility via a dedicated agent | EligibilityIdentifierAgent returns structured decision + reason |
| BR-05 | System must resolve required documents/timeline/fees via a dedicated agent | ProcedureResolverAgent returns structured procedure details |
| BR-06 | System must draft an official response combining both agent outputs | ResponseDrafterAgent produces citizen-facing text |
| BR-07 | Drafted responses must require officer approval before being finalized | CitizenRequest status transitions to AwaitingApproval, not Approved, by default |
| BR-08 | Officer must be able to approve or reject a drafted response | ApprovalController exposes approve/reject endpoints |
| BR-09 | System must isolate data between multiple government tenants (T0) | EF Core global query filters + automated cross-tenant test passing |
| BR-10 | System must provide real-time visibility into agent processing | SSE streaming endpoint emits live progress events |
| BR-11 | System must log correlation IDs across the full request lifecycle | Serilog structured logs include CorrelationId per request |

## 5. Out of Scope (for this MVP)

- Real user authentication/login system (currently using a fixed tenant ID for testing)
- OCR for scanned documents (only plain text/PDF text extraction covered)
- Production-grade vector database (Qdrant/Pinecone) — using SQL Server-based vector store as a documented MVP decision
- Multi-language support beyond Arabic (English UI labels only)
- Payment or fee-collection integration
- Mobile application

## 6. Business Rules

- BR-RULE-01: A response can never be sent to a citizen without explicit officer approval
- BR-RULE-02: If evidence for eligibility is insufficient, the system must escalate rather than guess
- BR-RULE-03: All data must be filtered by TenantId at the database level (not just application level)
- BR-RULE-04: An approval/rejection can only be performed on a request in "AwaitingApproval" status

## 7. Assumptions

- Google Gemini Free Tier is sufficient for MVP development and demo purposes
- A single fixed tenant is acceptable for development/testing; full multi-tenant onboarding UI is out of scope
- Government policy documents are provided as clean plain text (real-world PDF extraction complexity deferred)

## 8. Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Free-tier API rate limits interrupt development/demo | Medium | Retry-with-backoff implemented; evaluation harness paced with delays |
| LLM may occasionally produce inconsistent structured output | Medium | Strict prompt formatting + parsing with escalation fallback on unparseable output |
| SQL-based vector store does not scale beyond small corpora | Low (for MVP scope) | Documented as MVP trade-off in SDD gap table; Qdrant swap requires only IVectorStore adapter |


## 9. Traceability Matrix

| Requirement | Implemented? | Evidence |
|---|---|---|
| BR-01 (Ingestion) | ? Full | `IngestDocumentUseCase`, `POST /api/Ingest`, tested via Swagger with 4 real documents |
| BR-02 (Cited answers) | ? Full | `AskQuestionUseCase`, `EndToEndRagTests` passing |
| BR-03 (Refusal on low evidence) | ?? Partial | Evaluation harness implemented (25 Q/A pairs incl. 5 adversarial); 13/13 standard questions passed with 100% accuracy in latest run; adversarial refusal cases pending full run due to free-tier rate limits (documented in EVALUATION.md) || BR-04 (Eligibility agent) | ? Full | `EligibilityIdentifierAgent`, tested in `EligibilityAgentTests` |
| BR-05 (Procedure agent) | ? Full | `ProcedureResolverAgent`, verified via `ProcessRequestController` |
| BR-06 (Response drafting) | ? Full | `ResponseDrafterAgent`, verified end-to-end |
| BR-07 (Approval required) | ? Full | `CitizenRequest.AttachDraftedResponse` sets status to AwaitingApproval |
| BR-08 (Approve/Reject) | ? Full | `ApprovalController`, tested manually via Swagger |
| BR-09 (Tenant isolation) | ? Full | EF Core global query filters + `TenantIsolationTests` passing |
| BR-10 (SSE streaming) | ? Full | `ProcessRequestStreamController`, verified via browser |
| BR-11 (Correlation IDs) | ? Full | Serilog + `CorrelationIdMiddleware`, verified in console logs |