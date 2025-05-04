namespace EchoCore.Domain.Services
{
    public interface IPromptProfileService
    {
        Task<string> GetPromptTemplateAsync(string agentType, CancellationToken cancellationToken = default);
        Task<bool> CreatePromptProfileAsync(string agentType, string template, CancellationToken cancellationToken = default);
        Task<bool> UpdatePromptProfileAsync(string agentType, string updatedTemplate, CancellationToken cancellationToken = default);
        Task<bool> DeletePromptProfileAsync(string agentType, CancellationToken cancellationToken = default);
    }
}
