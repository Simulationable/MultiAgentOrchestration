using EchoCore.Domain.Entities;
using EchoCore.Domain.Factories;
using EchoCore.Domain.Models.Response;
using EchoCore.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Controllers
{
    /// <summary>
    /// Manages memory-linked projects for GPT thread grouping.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IHateoasFactory<Project> _hateoasFactory;

        public ProjectController(IProjectService projectService, IHateoasFactory<Project> hateoasFactory)
        {
            _projectService = projectService;
            _hateoasFactory = hateoasFactory;
        }

        /// <summary>
        /// Retrieves all projects with HATEOAS links included for self and related threads.
        /// </summary>
        /// <returns>A list of projects, each with HATEOAS links (self, threads)</returns>
        [HttpGet("hateoas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _projectService.GetAllAsync();

            var result = projects.Select(p => new
            {
                p.Id,
                p.Name,
                Links = _hateoasFactory.GetLinks(p)
            });

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all registered GPT memory projects.
        /// </summary>
        /// <returns>List of projects</returns>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Project>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        /// <summary>
        /// Creates a new memory project (used for grouping threads).
        /// </summary>
        /// <param name="input">Project object (must include name)</param>
        /// <returns>Created project with ID</returns>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProject([FromBody] Project input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                return BadRequest(new { error = "Project name is required." });

            var created = await _projectService.CreateProjectAsync(input);
            return Ok(created);
        }
    }
}
