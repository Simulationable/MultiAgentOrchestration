using EchoCore.Domain.Models.Agent;
using EchoCore.Domain.Models.Request;
using EchoCore.Domain.Services;

namespace EchoCore.Application.Services
{
    public class OrchestrationPlanner : IPlanningAgent
    {
        public Task<ExecutionPlan> BuildPlanAsync(AgentRunRequest request, CancellationToken cancellationToken = default)
        {
            var plan = new ExecutionPlan
            {
                Tasks = new List<AgentTask>
                {
                    new AgentTask { AgentType = "DataGatherer", Prompt = request.Prompt },
                    new AgentTask { AgentType = "Analyzer", Prompt = $"Analyze result from DataGatherer: {request.Prompt}" },
                    new AgentTask { AgentType = "Synthesizer", Prompt = $"Synthesize insights based on Analyzer: {request.Prompt}" },
                    new AgentTask { AgentType = "Communicator", Prompt = $"Communicate findings from Synthesizer: {request.Prompt}" }
                }
            };
            return Task.FromResult(plan);
        }
    }
}
