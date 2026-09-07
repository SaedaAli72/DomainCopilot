# ADR-003: Sequential Pipeline Orchestration Pattern

## Status: Accepted

## Context
The system requires coordinating 3 specialized agents (Eligibility Identifier, 
Procedure Resolver, Response Drafter) in a specific order, with the ability to 
stop early if human escalation is required.

## Decision
We implemented a **sequential pipeline pattern** in `GovernmentServiceOrchestrator`: 
each agent runs in strict order (Eligibility ? Procedure ? Response Drafter), with 
an early-exit check after the eligibility step. Agents communicate via typed 
contracts (`EligibilityResult`, `ProcedureResult`, `DraftedResponse`), not free-form 
text.

## Alternatives Considered
- **Supervisor pattern** (a controlling LLM agent decides which sub-agent to call 
  next dynamically): More flexible for complex, variable workflows. Rejected because 
  our D4 domain has a fixed, well-defined sequence (eligibility must be resolved 
  before procedure/response makes sense) — dynamic routing would add LLM cost and 
  non-determinism without benefit.
- **Planner-Executor pattern** (a planning agent generates a step sequence, then 
  executes it): Useful for open-ended tasks with variable steps. Rejected as 
  over-engineered for a fixed 3-step government workflow.
- **State machine** (explicit states or Approved/Rejected/Escalated with transition 
  rules): Considered for the CitizenRequest lifecycle itself (and partially adopted 
  via `RequestStatus` enum + guarded transition methods), but full state-machine 
  orchestration of agents was judged unnecessary complexity for a linear 3-step flow.

## Consequences
- **Positive**: Simple, predictable, easy to test and reason about; matches D4's 
  explicitly defined workflow (Eligibility ? Procedure ? Response) exactly as 
  specified in the assignment brief; early-exit on escalation avoids wasted LLM calls.
- **Negative**: Less flexible if future domains require dynamic agent selection; 
  adding a 4th agent requires a code change to the orchestrator (not configuration-driven).
- **Mandatory controls not yet implemented**: max-iteration breaker, per-step timeout, 
  and retry-with-backoff exist at the HTTP client level (Gemini calls) but not yet 
  at the orchestrator/agent level. Documented as a gap in SDD.