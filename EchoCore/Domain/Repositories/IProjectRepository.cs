using EchoCore.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Domain.Repositories
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project> CreateAsync(Project project);
    }
}
