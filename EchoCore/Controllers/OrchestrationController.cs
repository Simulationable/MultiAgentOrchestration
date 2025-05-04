using EchoCore.Domain.Models.Agent;
using EchoCore.Domain.Models.Request;
using EchoCore.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IMultiAgentOrchestrator _orchestrator;

        public OrchestrationController(IMultiAgentOrchestrator orchestrator)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        }

        /// <summary>
        /// Runs a multi-agent execution plan based on the input request.
        /// </summary>
        [HttpPost("run-plan")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RunPlan([FromBody] AgentRunRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AgentType) || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { error = "AgentType and Prompt are required." });
            }

            try
            {
                var orchestrationResult = await _orchestrator.RunPlanAsync(request, cancellationToken);

                return Ok(new
                {
                    finalOutput = orchestrationResult.FinalOutput,
                    tasks = (orchestrationResult.TaskResults ?? new List<AgentTaskResult>())
                        .Select(t => new
                        {
                            agentType = t.AgentType,
                            output = t.Output
                        }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
