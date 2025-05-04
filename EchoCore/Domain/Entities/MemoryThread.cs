using EchoCore.Domain.Models.Attributes;

namespace EchoCore.Domain.Entities
{
    [HateoasLink("self", "GetThreadById")]
    [HateoasLink("project", "GetProjectById")]
    [HateoasLink("update", "UpdateThread", "PUT")]
    [HateoasLink("delete", "DeleteThread", "DELETE")]
    public class MemoryThread : BaseEntity
    {
        public Guid ProjectId { get; set; }
        public string ThreadName { get; set; } = string.Empty;

        public Project? Project { get; set; }
        public ICollection<MemoryEntry> Entries { get; set; } = new List<MemoryEntry>();
        public ICollection<SemanticMemoryEntry> SemanticEntries { get; set; } = new List<SemanticMemoryEntry>();

    }
}
