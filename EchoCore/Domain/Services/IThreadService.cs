using EchoCore.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Domain.Services
{
    public interface IThreadService
    {
        Task<IEnumerable<MemoryThread>> GetThreadsByProjectIdAsync(Guid projectId);
        Task<MemoryThread> CreateThreadAsync(MemoryThread input);
    }
}
