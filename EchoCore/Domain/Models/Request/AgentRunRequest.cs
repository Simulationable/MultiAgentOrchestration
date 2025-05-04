using System.ComponentModel.DataAnnotations;

namespace EchoCore.Domain.Models.Request
{
    /// <summary>
    /// Payload structure for invoking a GPT-based agent with full configuration.
    /// </summary>
    public class AgentRunRequest
    {
        [Required]
        public Guid ThreadId { get; set; }

        [Required]
        public string AgentType { get; set; } = "dev-agent";

        [Required]
        public string Prompt { get; set; } = string.Empty;

        public string? SystemMessage { get; set; }

        /// <summary>
        /// Optional override: Which OpenAI model to use.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Optional override: Controls randomness (0.0 = deterministic, 1.0+ = creative).
        /// </summary>
        [Range(0.0, 2.0)]
        public double? Temperature { get; set; }

        /// <summary>
        /// Optional override: Max tokens for GPT response.
        /// </summary>
        [Range(100, 8000)]
        public int? MaxTokens { get; set; }
    }
}
