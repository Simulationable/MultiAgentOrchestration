using EchoCore.Domain.Models.Response;

namespace EchoCore.Domain.Factories
{
    /// <summary>
    /// Defines a contract for generating HATEOAS links for a resource.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    public interface IHateoasFactory<T>
    {
        /// <summary>
        /// Builds HATEOAS links for a given resource.
        /// </summary>
        /// <param name="resource">The resource entity.</param>
        /// <returns>A list of LinkDto representing available actions.</returns>
        List<LinkDto> GetLinks(T resource);
    }
}
