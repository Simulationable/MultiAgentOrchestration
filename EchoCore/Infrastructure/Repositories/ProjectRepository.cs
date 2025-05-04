using EchoCore.Domain.Entities;
using EchoCore.Domain.Repositories;
using EchoCore.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EchoCore.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly MemoryDbContext _db;

        public ProjectRepository(MemoryDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _db.Projects.ToListAsync();
        }

        public async Task<Project> CreateAsync(Project project)
        {
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project;
        }
    }
}
