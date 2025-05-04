using EchoCore.Domain.Models.Attributes;

namespace EchoCore.Domain.Entities
{
    [HateoasLink("self", "GetProjectById")]
    [HateoasLink("threads", "GetThreadsByProject")]
    public class Project : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<MemoryThread> Threads { get; set; } = new List<MemoryThread>();
    }
}
