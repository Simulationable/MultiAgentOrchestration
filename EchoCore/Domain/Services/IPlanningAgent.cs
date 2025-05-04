using EchoCore.Domain.Models.Agent;
using EchoCore.Domain.Models.Request;

namespace EchoCore.Domain.Services
{
    public interface IPlanningAgent
    {
        Task<ExecutionPlan> BuildPlanAsync(AgentRunRequest request, CancellationToken cancellationToken = default);
    }
}
