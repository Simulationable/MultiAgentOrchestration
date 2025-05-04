using EchoCore.Domain.Entities;
using EchoCore.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Controllers
{
    /// <summary>
    /// Manages GPT memory threads within a project.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ThreadController : ControllerBase
    {
        private readonly IThreadService _threadService;

        public ThreadController(IThreadService threadService)
        {
            _threadService = threadService;
        }

        /// <summary>
        /// Retrieves all memory threads under a specific project.
        /// </summary>
        /// <param name="projectId">The GUID of the project</param>
        /// <returns>List of memory threads</returns>
        [HttpGet("project/{projectId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<MemoryThread>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetThreads(Guid projectId)
        {
            var threads = await _threadService.GetThreadsByProjectIdAsync(projectId);
            return Ok(threads);
        }

        /// <summary>
        /// Creates a new GPT memory thread under an existing project.
        /// </summary>
        /// <param name="input">Thread object (must include projectId)</param>
        /// <returns>Created thread with ID</returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(MemoryThread), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateThread([FromBody] MemoryThread input)
        {
            if (input.ProjectId == Guid.Empty)
                return BadRequest(new { error = "ProjectId is required." });

            var created = await _threadService.CreateThreadAsync(input);
            return Ok(created);
        }
    }
}
