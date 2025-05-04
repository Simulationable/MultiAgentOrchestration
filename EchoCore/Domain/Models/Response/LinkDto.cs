namespace EchoCore.Domain.Models.Response
{
    /// <summary>
    /// Represents a HATEOAS link with href, relation, and HTTP method.
    /// </summary>
    public class LinkDto
    {
        public LinkDto(string href, string rel)
        {
            Href = href;
            Rel = rel;
            Method = "GET"; // default ถ้าไม่ส่ง method
        }

        public LinkDto(string href, string rel, string method)
        {
            Href = href;
            Rel = rel;
            Method = method;
        }

        /// <summary>
        /// The URI of the linked resource.
        /// </summary>
        public string Href { get; set; } = default!;

        /// <summary>
        /// The relation of the link (e.g. "self", "project", "create-thread").
        /// </summary>
        public string Rel { get; set; } = default!;

        /// <summary>
        /// The HTTP method to be used with this link (e.g. GET, POST, DELETE).
        /// </summary>
        public string Method { get; set; } = default!;
    }
}
