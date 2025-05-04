using OpenAI.Chat;
using System.Threading;

namespace EchoCore.Domain.Services
{
    public interface IGptClient
    {
        Task<string> GetChatCompletionAsync(List<Message> messages, CancellationToken cancellationToken = default);
        Task<string> GetChatCompletionAsync(List<Message> messages, string? model, double? temperature, CancellationToken cancellationToken = default);
        Task<string> GetChatCompletionAsync(List<Message> messages, string? model, double? temperature, int? maxTokens, CancellationToken cancellationToken = default);
        Task<string> GetChatCompletionWithImageAsync(List<Message> priorMessages, string prompt, byte[] imageBytes, string model, int maxTokens, CancellationToken cancellationToken = default);
    }
}
