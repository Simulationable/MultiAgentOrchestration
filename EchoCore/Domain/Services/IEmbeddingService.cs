using System.Threading;
using System.Threading.Tasks;

namespace EchoCore.Domain.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default);
    }
}
