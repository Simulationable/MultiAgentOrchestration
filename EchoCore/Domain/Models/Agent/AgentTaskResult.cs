using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Domain.Models.Agent
{
    public class AgentTaskResult
    {
        public string? AgentType { get; set; }
        public string? Output { get; set; }
    }
}
