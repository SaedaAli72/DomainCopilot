# ADR-004: EF Core Global Query Filters for Multi-Tenant Isolation (T0)

## Status: Accepted

## Context
The assigned twist (T0) requires strict isolation between ?2 tenants' corpora, 
users, and runs, enforced at the data layer, with a test proving cross-tenant 
leakage is impossible.

## Decision
We implemented tenant isolation using **EF Core Global Query Filters**, applied in 
`AppDbContext.OnModelCreating`. Every tenant-scoped entity (`CitizenRequest`, 
`Document`) is filtered by `TenantId == _tenantProvider.GetCurrentTenantId()` 
automatically, for every query, regardless of where in the codebase the query is 
written. Isolation is verified by `TenantIsolationTests`, which seeds two tenants 
in the same in-memory database and asserts that querying as Tenant A never returns 
Tenant B's data.

## Alternatives Considered
- **Separate databases per tenant**: Strongest isolation guarantee (physical 
  separation). Rejected for MVP due to operational complexity of provisioning a new 
  database per tenant and running migrations across N databases within the 12-day window.
- **Manual `.Where(TenantId == ...)` on every query**: Simple to understand, but 
  rejected because it relies on developer discipline — a single forgotten `.Where()` 
  clause anywhere in the codebase would silently leak data across tenants, which is 
  precisely the risk T0 warns against.
- **Separate schemas per tenant (same database)**: A middle ground between full 
  isolation and shared tables. Rejected for MVP scope; documented as a stronger 
  future option if the system needs to scale beyond a handful of tenants.

## Consequences
- **Positive**: Isolation is enforced structurally, not by convention — even a new 
  developer unfamiliar with the codebase cannot accidentally leak data, because EF 
  Core applies the filter transparently to every query. Proven by an automated test, 
  not just manual verification.
- **Negative**: Shared tables mean a single database compromise affects all tenants 
  (no physical blast-radius containment). A known EF Core limitation was encountered: 
  global query filters on required relationships (e.g., `Document` ? `DocumentChunk`) 
  can produce unexpected results if not filtered consistently on both sides — 
  documented and worked around during implementation.
- **Documented gap**: Physical database-per-tenant separation would strengthen the 
  isolation guarantee further; deferred as unnecessary for MVP demo scale (2-3 tenants).