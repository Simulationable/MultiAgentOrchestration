namespace EchoCore.Domain.Entities
{
    public class SemanticMemoryEntry : BaseEntity
    {
        public Guid ThreadId { get; set; }
        public string Content { get; set; } = string.Empty;
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public MemoryThread? Thread { get; set; }
    }
}
