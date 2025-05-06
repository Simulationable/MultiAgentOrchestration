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
            if (entry == null)
            {
                _logger.LogWarning("Attempted to add null SemanticMemoryEntry.");
                throw new ArgumentNullException(nameof(entry));
            }

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
            if (queryEmbedding == null || queryEmbedding.Length == 0)
            {
                _logger.LogWarning("SearchSimilarAsync called with null or empty queryEmbedding.");
                return new List<SemanticMemoryEntry>();
            }

            var entries = await _db.Set<SemanticMemoryEntry>()
                .AsNoTracking()
                .Where(e => e.ThreadId == threadId)
                .ToListAsync(cancellationToken);

            if (!entries.Any())
            {
                _logger.LogInformation("No semantic memory entries found for thread {ThreadId}.", threadId);
                return new List<SemanticMemoryEntry>();
            }

            var scoredEntries = entries
                .Select(e =>
                {
                    if (e.Embedding == null || e.Embedding.Length != queryEmbedding.Length)
                    {
                        return null;
                    }

                    var similarity = CosineSimilarity(e.Embedding, queryEmbedding);

                    var weightedScore = similarity;

                    return new
                    {
                        Entry = e,
                        Score = weightedScore
                    };
                })
                .Where(x => x != null && x.Score >= 0)
                .Select(x => x!.Entry)
                .Take(topN)
                .ToList();

            _logger.LogInformation("SearchSimilarAsync: Found {Count} top matches for thread {ThreadId}.", scoredEntries.Count, threadId);

            return scoredEntries;
        }

        private double CosineSimilarity(float[] vec1, float[] vec2)
        {
            if (vec1 == null || vec2 == null || vec1.Length != vec2.Length)
            {
                return 0;
            }

            double dot = 0.0;
            double mag1 = 0.0;
            double mag2 = 0.0;

            for (int i = 0; i < vec1.Length; i++)
            {
                dot += vec1[i] * vec2[i];
                mag1 += vec1[i] * vec1[i];
                mag2 += vec2[i] * vec2[i];
            }

            if (mag1 == 0 || mag2 == 0)
            {
                return 0;
            }

            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }
    }
}
