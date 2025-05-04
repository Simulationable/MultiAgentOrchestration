using EchoCore.Domain.Entities;

namespace EchoCore.Domain.Repositories
{
    public interface IPromptProfileRepository
    {
        Task<PromptProfile> GetByAgentTypeAsync(string agentType);
        Task AddAsync(PromptProfile profile);
        Task UpdateAsync(PromptProfile profile);
        Task DeleteAsync(PromptProfile profile);
    }
}
