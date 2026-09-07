# System Design Document — Domain Copilot

## Part A: Target Architecture (Unconstrained)

In an unconstrained production environment, Domain Copilot would include:

- **API Gateway** with managed rate limiting (e.g., Azure API Management) to protect against abuse and enable per-tenant throttling
- **Secrets Manager** (e.g., Azure Key Vault) instead of local User Secrets, with automatic key rotation
- **Message Broker** (e.g., Azure Service Bus) for async long-running agent workflows, decoupling request handling from processing
- **Managed Vector Database** (e.g., Qdrant Cloud or Azure AI Search) supporting millions of chunks with sub-100ms retrieval
- **Autoscaling compute** (Azure Container Apps / AKS) to handle variable citizen request load
- **Redis caching layer** for frequently-asked eligibility criteria to reduce LLM calls
- **Full observability stack** (OpenTelemetry + Grafana/Application Insights) with distributed tracing across all agent calls
- **Multi-region deployment** with database replication for disaster recovery
- **CI/CD pipeline** with staging/production environments and blue-green deployments
- **Estimated cost at scale** (10,000 requests/day): ~$800-1,500/month (compute + managed vector DB + LLM API costs)

## Part B: Implemented MVP

The current implementation targets a working, correctly-architected MVP within the 
12-day assessment window, prioritizing correctness of the core RAG/Agent pipeline 
and Clean Architecture boundaries over infrastructure scale. All target-vs-MVP gaps 
are documented below with justification and closure effort.

## Part B: Implemented MVP — Gap Table

| Target Component | Implemented? | Why Deferred | Interim Mitigation | Effort to Close |
|---|---|---|---|---|
| Production LLM/Embedding provider (paid tier, higher rate limits) | ? No | Free tier sufficient for MVP demo scope; budget constraint (assessment requires $0 cost) | Retry-with-backoff implemented to handle rate limits gracefully | ~1h + $X/month (paid tier) |
| Managed Vector Database (Qdrant/Pinecone) | ? No | Time constraint (12-day window); SQL Server sufficient for small demo corpus (30 docs) | Cosine similarity computed in-memory via SqlVectorStore, isolated behind IVectorStore interface | ~4h (implement QdrantVectorStore adapter; interface already abstracts this) |
| Real user authentication (JWT/OAuth) | ? No | Out of scope per BRD; focus prioritized on core RAG/Agent correctness | Fixed StaticTenantProvider for demo/testing purposes | ~6-8h (ASP.NET Identity + JWT + tenant claim resolution) |