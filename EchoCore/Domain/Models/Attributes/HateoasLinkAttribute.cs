namespace EchoCore.Domain.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HateoasLinkAttribute : Attribute
    {
        public HateoasLinkAttribute(string rel, string routeName, string method = "GET")
        {
            Rel = rel;
            RouteName = routeName;
            Method = method;
        }

        public string Rel { get; }
        public string RouteName { get; }
        public string Method { get; }

    }

}
