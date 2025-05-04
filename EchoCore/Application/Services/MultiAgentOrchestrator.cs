using EchoCore.Domain.Models.Agent;
using EchoCore.Domain.Models.Request;
using EchoCore.Domain.Repositories;
using EchoCore.Domain.Services;

namespace EchoCore.Application.Services
{
    public class MultiAgentOrchestrator : IMultiAgentOrchestrator
    {
        private readonly IPlanningAgent _planningAgent;
        private readonly IAgentRunner _agentRunner;
        private readonly IPromptProfileRepository _agentRegistry;
        private readonly ILogger<MultiAgentOrchestrator> _logger;

        public MultiAgentOrchestrator(
            IPlanningAgent planningAgent,
            IAgentRunner agentRunner,
            IPromptProfileRepository agentRegistry,
            ILogger<MultiAgentOrchestrator> logger)
        {
            _planningAgent = planningAgent;
            _agentRunner = agentRunner;
            _agentRegistry = agentRegistry;
            _logger = logger;
        }

        public async Task<OrchestrationResult> RunPlanAsync(AgentRunRequest request, CancellationToken cancellationToken = default)
        {
            var plan = await _planningAgent.BuildPlanAsync(request, cancellationToken);
            var executionResults = new List<AgentTaskResult>();
            string? previousResult = null;

            foreach (var task in plan.Tasks)
            {
                var agentProfile = await _agentRegistry.GetByAgentTypeAsync(task.AgentType.ToLower());
                if (agentProfile == null)
                {
                    _logger.LogWarning("No agent registered for type {AgentType}", task.AgentType);
                    continue;
                }

                var chainedPrompt = $"{task.Prompt}\n\n[Previous Result]: {previousResult}";

                var taskRequest = new AgentRunRequest
                {
                    ThreadId = request.ThreadId,
                    AgentType = task.AgentType,
                    Prompt = chainedPrompt,
                    SystemMessage = request.SystemMessage,
                    Temperature = request.Temperature,
                    MaxTokens = request.MaxTokens,
                    Model = request.Model
                };

                _logger.LogInformation("MultiAgentOrchestrator executing AgentType: {AgentType}", task.AgentType);
                var result = await _agentRunner.RunAsync(taskRequest, cancellationToken);

                executionResults.Add(new AgentTaskResult
                {
                    AgentType = task.AgentType,
                    Output = result
                });

                previousResult = result;
            }

            return new OrchestrationResult
            {
                FinalOutput = previousResult,
                TaskResults = executionResults
            };
        }
    }
}
