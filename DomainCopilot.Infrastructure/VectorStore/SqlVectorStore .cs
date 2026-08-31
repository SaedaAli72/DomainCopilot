
using DomainCopilot.Application.Interfaces;
using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.VectorStore
{
    public class SqlVectorStore :IVectorStore
    {
        private readonly AppDbContext _context;

        public SqlVectorStore(AppDbContext context)
        {
            _context = context;
        }

        // Stores the embedding for a chunk by converting float[] to byte[] and saving it
        public async Task IndexAsync(Guid chunkId, float[] embedding, Guid tenantId, CancellationToken cancellationToken = default)
        {
            var chunk = await _context.DocumentChunks.FindAsync(new object[] { chunkId }, cancellationToken);
            if (chunk is null) return;

            var bytes = new byte[embedding.Length * sizeof(float)];
            Buffer.BlockCopy(embedding, 0, bytes, 0, bytes.Length);

            _context.Entry(chunk).Property("Embedding").CurrentValue = bytes;
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Finds the topK closest chunks to the query, within the same tenant, using cosine similarity
        public async Task<List<Guid>> SearchAsync(float[] queryEmbedding, Guid tenantId, int topK, CancellationToken cancellationToken = default)
        {
            var candidates = await _context.DocumentChunks
                .Where(c => c.Document!.TenantId == tenantId)
                .Select(c => new { c.Id, EmbeddingBytes = EF.Property<byte[]>(c, "Embedding") })
                .Where(x => x.EmbeddingBytes != null)
                .ToListAsync(cancellationToken);

            var ranked = candidates
                .Select(c => new
                {
                    c.Id,
                    Score = CosineSimilarity(queryEmbedding, ToFloatArray(c.EmbeddingBytes!))
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => x.Id)
                .ToList();

            return ranked;
        }

        // Converts stored bytes back into a float array
        private static float[] ToFloatArray(byte[] bytes)
        {
            var floats = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
            return floats;
        }

        // Measures how "close" two embeddings are in meaning (1 = identical, 0 = unrelated)
        private static float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }
            return dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
        }
    }
}
