# AI Usage Log — Domain Copilot

## What Was Delegated to AI (Claude)

- Initial code scaffolding for all layers (Domain entities, Application 
  interfaces/use cases, Infrastructure implementations)
- Explanations of C# concepts, EF Core patterns, and architectural principles 
  as I learned them
- Draft prompts for the 3 agents (Eligibility, Procedure, Response Drafter)
- Documentation drafts (BRD, SDD, ADRs, SECURITY.md, ARCHITECTURE.md diagrams)
- Debugging assistance when errors occurred

## What I Wrote/Decided Myself

- Final approval of every architectural decision (e.g., choosing SQL-based 
  vector store over Qdrant for MVP scope — I confirmed this trade-off)
- All actual typing of code into Visual Studio (no copy-paste of entire files 
  without understanding each line — verified via checkpoint Q&A during development)
- Domain variant confirmation (D4 Government + T0 Multi-tenancy) from the 
  official assignment email
- Test data content (golden set questions, seeded regulation documents)
- Final review and correction of every AI-generated file before committing

## Where AI Misled Me (Documented Honestly)

### 1. Outdated Gemini API details
**What happened**: The first `GeminiEmbeddingService` implementation used an 
incorrect model name (`text-embedding-004`) and authentication method 
(API key in URL query string), based on outdated training data. This caused 
a 404 error on first real API call.

**How I verified/fixed**: AI searched current official Google documentation, 
found the correct model (`gemini-embedding-001`) and authentication method 
(`x-goog-api-key` header), and we fixed it together. This was caught 
immediately because we tested with a real API call rather than trusting the 
code blindly.

### 2. EF Core Migration factory pattern
**What happened**: The `AppDbContextFactory` needed for design-time migrations 
wasn't anticipated upfront; `Add-Migration` failed with a confusing 
"Object reference not set" error that required real debugging (including a 
`-Verbose` flag investigation) to trace back to a `float[]` shadow property 
conflicting with EF Core 8's primitive collection conventions.

**How I verified/fixed**: Ran the actual error with verbose output, read the 
full stack trace together, and iteratively narrowed down the cause (changing 
`float[]` to `byte[]` for the shadow property resolved it).

### 3. Underestimated Gemini free-tier rate limits
**What happened**: AI initially assumed generous free-tier quotas (hundreds of 
requests/day) for the Evaluation Harness. The actual limit was much stricter 
(~20 requests per rolling window), causing repeated 429 errors across multiple 
sessions.

**How I verified/fixed**: Captured the actual error response body from Google 
(not just the status code), which revealed the exact quota metric and value. 
Implemented retry-with-exponential-backoff as a real engineering fix, and 
honestly documented the remaining limitation in `docs/EVALUATION.md` rather 
than hiding it.

## How I Verified AI Output Throughout

- Every code block was explained line-by-line before I typed it, with 
  checkpoint questions to confirm understanding
- Every service (Embedding, LLM, VectorStore) was tested individually via 
  unit/integration tests before being wired into the full pipeline
- Manual testing via Swagger confirmed each endpoint's real behavior before 
  moving to the next feature
- The full end-to-end RAG pipeline was validated with a real integration test 
  (`EndToEndRagTests`) asserting a grounded, correct answer — not just "no errors"