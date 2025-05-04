using EchoCore.Domain.Repositories;
using EchoCore.Domain.Services;
using System.Collections.Concurrent;

namespace EchoCore.Infrastructure.Repositories
{
    public class DistributedAgentRegistry : IAgentRegistry
    {
        private readonly ConcurrentDictionary<string, IAgentRunner> _agents = new();

        public Task RegisterAgentAsync(string agentType, IAgentRunner agentRunner)
        {
            _agents[agentType] = agentRunner;
            return Task.CompletedTask;
        }

        public IAgentRunner? GetAgent(string agentType)
        {
            _agents.TryGetValue(agentType, out var runner);
            return runner;
        }
    }
}
