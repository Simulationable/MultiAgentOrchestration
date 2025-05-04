using EchoCore.Domain.Factories;
using EchoCore.Domain.Models.Attributes;
using EchoCore.Domain.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace EchoCore.Infrastructure.Factories
{
    public class DynamicHateoasFactory<T> : IHateoasFactory<T>
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        public DynamicHateoasFactory(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        public List<LinkDto> GetLinks(T resource)
        {
            var links = new List<LinkDto>();

            var type = typeof(T);
            var attributes = type.GetCustomAttributes(typeof(HateoasLinkAttribute), inherit: true)
                .Cast<HateoasLinkAttribute>();

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext!);

            foreach (var attr in attributes)
            {
                var routeValues = new Dictionary<string, object>();

                foreach (var prop in type.GetProperties())
                {
                    if (attr.RouteName.Contains(prop.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        var value = prop.GetValue(resource);
                        if (value is not null)
                        {
                            routeValues[prop.Name] = value;
                        }
                    }
                }

                var href = urlHelper.Link(attr.RouteName, routeValues);
                if (href != null)
                {
                    links.Add(new LinkDto(href, attr.Rel, attr.Method));
                }
            }

            return links;
        }
    }
}
