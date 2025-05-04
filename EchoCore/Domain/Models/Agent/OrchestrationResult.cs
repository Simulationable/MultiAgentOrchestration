using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Domain.Models.Agent
{
    public class OrchestrationResult
    {
        public string? FinalOutput { get; set; }
        public List<AgentTaskResult>? TaskResults { get; set; }
    }
}
