using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EchoCore.Domain.Entities;
using EchoCore.Domain.Models.Request;

namespace EchoCore.Domain.Services
{
    public interface IAgentRunner
    {
        Task<string> RunAsync(AgentRunRequest request, CancellationToken cancellationToken = default);
        Task<string> RunWithImageAsync(AgentRunRequest request, byte[] imageBytes, CancellationToken cancellationToken = default);
        Task<IEnumerable<MemoryEntry>> GetHistoryAsync(Guid threadId, int page, CancellationToken cancellationToken = default);
    }
}
