using EchoCore.Domain.Entities;
using System.Threading;

namespace EchoCore.Domain.Repositories
{
    public interface IMemoryRepository
    {
        /// <summary>
        /// Get a memory thread along with its entries.
        /// </summary>
        /// <param name="threadId">Thread identifier.</param>
        /// <param name="asNoTracking">Disable EF change tracking for performance.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The memory thread or null if not found.</returns>
        Task<MemoryThread?> GetThreadWithEntriesAsync(Guid threadId, bool asNoTracking = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add a memory entry to the context (does not save immediately).
        /// </summary>
        /// <param name="entry">Memory entry to add.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task AddEntryAsync(MemoryEntry entry, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist all changes to the database.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
