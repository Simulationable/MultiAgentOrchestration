using EchoCore.Domain.Entities;
using EchoCore.Domain.Repositories;
using EchoCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EchoCore.Infrastructure.Repositories
{
    public class PromptProfileRepository : IPromptProfileRepository
    {
        private readonly MemoryDbContext _dbContext;

        public PromptProfileRepository(MemoryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PromptProfile> GetByAgentTypeAsync(string agentType)
        {
            var profile = await _dbContext.PromptProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.AgentType!.ToLower() == agentType);

            if (profile == null)
                throw new KeyNotFoundException($"Prompt profile '{agentType}' not found.");

            return profile;
        }

        public async Task AddAsync(PromptProfile profile)
        {
            await _dbContext.PromptProfiles.AddAsync(profile);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(PromptProfile profile)
        {
            _dbContext.PromptProfiles.Update(profile);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(PromptProfile profile)
        {
            _dbContext.PromptProfiles.Remove(profile);
            await _dbContext.SaveChangesAsync();
        }
    }
}
