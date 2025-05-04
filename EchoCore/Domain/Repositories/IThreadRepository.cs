using EchoCore.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Domain.Repositories
{
    public interface IThreadRepository
    {
        Task<IEnumerable<MemoryThread>> GetByProjectIdAsync(Guid projectId);
        Task<MemoryThread> CreateAsync(MemoryThread thread);
    }
}
