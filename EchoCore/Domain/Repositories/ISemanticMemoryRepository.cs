using EchoCore.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EchoCore.Domain.Repositories
{
    public interface ISemanticMemoryRepository
    {
        /// <summary>
        /// Add a semantic memory entry to the context (does not save immediately).
        /// </summary>
        /// <param name="entry">Semantic memory entry.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task Add(SemanticMemoryEntry entry, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist all pending semantic memory changes to the database.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Search for the top N most similar semantic memory entries within a thread using cosine similarity.
        /// </summary>
        /// <param name="threadId">Thread identifier.</param>
        /// <param name="queryEmbedding">Query embedding vector.</param>
        /// <param name="topN">Number of top matches to return.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>List of top similar SemanticMemoryEntry objects.</returns>
        Task<List<SemanticMemoryEntry>> SearchSimilarAsync(Guid threadId, float[] queryEmbedding, int topN = 3, CancellationToken cancellationToken = default);
    }
}
