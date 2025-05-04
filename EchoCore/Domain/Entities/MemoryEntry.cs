namespace EchoCore.Domain.Entities
{
    public class MemoryEntry : BaseEntity
    {
        public Guid ThreadId { get; set; }
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
        public MemoryThread? Thread { get; set; }
    }
}
