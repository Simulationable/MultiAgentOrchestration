namespace EchoCore.Domain.Entities
{
    public class PromptProfile : BaseEntity
    {
        public string? AgentType { get; set; }
        public string? Template { get; set; }
    }
}
