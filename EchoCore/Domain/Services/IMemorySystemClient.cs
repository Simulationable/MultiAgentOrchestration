using System;
using System.Threading.Tasks;

namespace EchoCore.Domain.Services
{
    public interface IMemorySystemClient
    {
        /// <summary>
        /// Runs a memory-based chat interaction on the specified thread.
        /// </summary>
        /// <param name="threadId">The unique identifier of the memory thread.</param>
        /// <param name="userInput">The user’s input prompt.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The assistant’s generated response.</returns>
        Task<string> ChatWithMemoryAsync(Guid threadId, string userInput, CancellationToken cancellationToken = default);
    }
}
