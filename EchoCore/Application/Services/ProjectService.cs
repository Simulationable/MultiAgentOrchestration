using EchoCore.Domain.Entities;
using EchoCore.Domain.Repositories;
using EchoCore.Domain.Services;
using EchoCore.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repo;

        public ProjectService(IProjectRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Project> CreateProjectAsync(Project input)
        {
            return await _repo.CreateAsync(input);
        }
    }
}
