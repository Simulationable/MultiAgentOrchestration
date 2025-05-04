using EchoCore.Domain.Entities;
using EchoCore.Domain.Repositories;
using EchoCore.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Application.Services
{
    public class ThreadService : IThreadService
    {
        private readonly IThreadRepository _repo;

        public ThreadService(IThreadRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<MemoryThread>> GetThreadsByProjectIdAsync(Guid projectId)
        {
            return await _repo.GetByProjectIdAsync(projectId);
        }

        public async Task<MemoryThread> CreateThreadAsync(MemoryThread input)
        {
            return await _repo.CreateAsync(input);
        }
    }
}
