using EchoCore.Domain.Models.Agent;
using EchoCore.Domain.Models.Request;

namespace EchoCore.Domain.Services
{
    public interface IMultiAgentOrchestrator
    {
        Task<OrchestrationResult> RunPlanAsync(AgentRunRequest request, CancellationToken cancellationToken = default);
    }
}
