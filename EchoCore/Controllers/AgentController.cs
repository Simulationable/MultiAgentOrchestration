using EchoCore.Application.Services;
using EchoCore.Domain.Models.Request;
using EchoCore.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;

namespace EchoCore.Controllers
{
    /// <summary>
    /// Executes agent logic (e.g., Companion AI, Dev Assistant) with full GPT config.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentRunner _agentRunner;

        public AgentController(IAgentRunner agentRunner)
        {
            _agentRunner = agentRunner ?? throw new ArgumentNullException(nameof(agentRunner));
        }

        /// <summary>
        /// Sends a prompt to a specific agent with full options (model, temperature, etc.).
        /// </summary>
        [HttpPost("run")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RunAgent([FromBody] AgentRunRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AgentType) || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { error = "AgentType and Prompt are required." });
            }

            var result = await _agentRunner.RunAsync(request, cancellationToken);
            return Ok(new { response = result });
        }

        /// <summary>
        /// Sends an image + prompt to a GPT agent (with memory + agent persona).
        /// Use this for GPT-4o Vision use-cases.
        /// </summary>
        [HttpPost("run-image")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RunAgentWithImage([FromForm] AgentRunRequest request, IFormFile image, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Request data is required." });
            }

            if (image == null || image.Length == 0)
            {
                return BadRequest(new { error = "Image is required." });
            }

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms, cancellationToken);
            var imageBytes = ms.ToArray();

            var result = await _agentRunner.RunWithImageAsync(request, imageBytes, cancellationToken);
            return Ok(new { response = result });
        }

        /// <summary>
        /// Gets historical messages for a given thread (used for lazy-loading chat).
        /// </summary>
        [HttpGet("history")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetHistory([FromQuery] Guid threadId, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
        {
            if (threadId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid threadId." });
            }

            var messages = await _agentRunner.GetHistoryAsync(threadId, page, cancellationToken);
            return Ok(new { messages });
        }
    }
}
