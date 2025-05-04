using EchoCore.Domain.Entities;
using EchoCore.Domain.Repositories;
using EchoCore.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EchoCore.Infrastructure.Repositories
{
    public class ThreadRepository : IThreadRepository
    {
        private readonly MemoryDbContext _db;

        public ThreadRepository(MemoryDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<MemoryThread>> GetByProjectIdAsync(Guid projectId)
        {
            return await _db.Threads.Where(t => t.ProjectId == projectId).ToListAsync();
        }

        public async Task<MemoryThread> CreateAsync(MemoryThread thread)
        {
            _db.Threads.Add(thread);
            await _db.SaveChangesAsync();
            return thread;
        }
    }
}
