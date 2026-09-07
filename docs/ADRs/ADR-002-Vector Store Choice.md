# ADR-002: SQL Server-Based Vector Store (Instead of a Dedicated Vector Database)

## Status: Accepted

## Context
The RAG pipeline requires storing embeddings and performing similarity search to 
retrieve relevant document chunks. Dedicated vector databases (Qdrant, Pinecone) 
are the industry-standard solution for this.

## Decision
We implemented `SqlVectorStore`, which stores embeddings as `byte[]` in a shadow 
property on `DocumentChunk` (in SQL Server), and computes cosine similarity in-memory 
in C# at query time, ranking and returning the top-K closest chunks.

## Alternatives Considered
- **Qdrant (self-hosted via Docker)**: Industry-standard, purpose-built for vector 
  search, scales to millions of vectors. Rejected for MVP due to added infrastructure 
  complexity (Docker setup, networking, backup) within the 12-day window, and because 
  our test corpus (4 documents, ~10 chunks) does not require its performance benefits.
- **Pinecone (managed cloud)**: Fully managed, no infrastructure to run. Rejected 
  because it requires a paid plan beyond free-tier limits for production use, 
  conflicting with the "no paid subscriptions required" constraint.
- **In-memory only (no persistence)**: Simplest option. Rejected because embeddings 
  must survive application restarts and be queryable per-tenant from the database.

## Consequences
- **Positive**: Zero additional infrastructure; embeddings persist in the same 
  database as everything else; fully isolated behind the `IVectorStore` interface, 
  so swapping to Qdrant later requires only a new adapter class and one line in 
  `Program.cs` — no changes to `Application` layer logic.
- **Negative**: Brute-force cosine similarity does not scale beyond a few thousand 
  chunks; retrieval time grows linearly with corpus size.
- **Documented gap**: See `SDD.md` Gap Table — closing this gap is estimated at 
  ~4 hours (implement `QdrantVectorStore : IVectorStore`).