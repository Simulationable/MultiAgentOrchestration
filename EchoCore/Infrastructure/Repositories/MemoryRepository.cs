using EchoCore.Domain.Entities;
using EchoCore.Domain.Repositories;
using EchoCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EchoCore.Infrastructure.Repositories
{
    public class MemoryRepository : IMemoryRepository
    {
        private readonly MemoryDbContext _db;
        private readonly ILogger<MemoryRepository> _logger;

        public MemoryRepository(MemoryDbContext db, ILogger<MemoryRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<MemoryThread?> GetThreadWithEntriesAsync(Guid threadId, bool asNoTracking = false, CancellationToken cancellationToken = default)
        {
            var query = _db.Threads.Include(t => t.Entries)
                                   .Where(t => t.Id == threadId);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public Task AddEntryAsync(MemoryEntry entry, CancellationToken cancellationToken = default)
        {
            _db.Entries.Add(entry);
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
                _logger.LogError(ex, "Database update failed.");
                throw;
            }
        }
    }
}
