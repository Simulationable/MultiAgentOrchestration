using EchoCore.Domain.Entities;
using EchoCore.Domain.Repositories;
using EchoCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EchoCore.Infrastructure.Repositories
{
    public class SemanticMemoryRepository : ISemanticMemoryRepository
    {
        private readonly MemoryDbContext _db;
        private readonly ILogger<SemanticMemoryRepository> _logger;

        public SemanticMemoryRepository(MemoryDbContext db, ILogger<SemanticMemoryRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public Task Add(SemanticMemoryEntry entry, CancellationToken cancellationToken = default)
        {
            _db.Set<SemanticMemoryEntry>().Add(entry);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to save semantic memory changes.");
                throw;
            }
        }

        public async Task<List<SemanticMemoryEntry>> SearchSimilarAsync(Guid threadId, float[] queryEmbedding, int topN = 3, CancellationToken cancellationToken = default)
        {
            var entries = await _db.Set<SemanticMemoryEntry>()
                .AsNoTracking()
                .Where(e => e.ThreadId == threadId)
                .ToListAsync(cancellationToken);

            var scoredEntries = entries
                .Select(e => new
                {
                    Entry = e,
                    Score = CosineSimilarity(e.Embedding, queryEmbedding)
                })
                .Where(x => x.Score >= 0)
                .OrderByDescending(x => x.Score)
                .Take(topN)
                .Select(x => x.Entry)
                .ToList();

            _logger.LogInformation("SearchSimilarAsync: Found {Count} top matches for thread {ThreadId}", scoredEntries.Count, threadId);

            return scoredEntries;
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return -1f;

            float dot = 0f, magA = 0f, magB = 0f;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            var denominator = Math.Sqrt(magA) * Math.Sqrt(magB);
            if (denominator < 1e-8) return -1f;

            var cosine = dot / (float)(denominator);
            return cosine;
        }
    }
}
