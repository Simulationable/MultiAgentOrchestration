using EchoCore.Domain.Services;

namespace EchoCore.Domain.Repositories
{
    public interface IAgentRegistry
    {
        Task RegisterAgentAsync(string agentType, IAgentRunner agentRunner);
        IAgentRunner? GetAgent(string agentType);
    }
}
