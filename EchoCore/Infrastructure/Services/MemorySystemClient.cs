using EchoCore.Domain.Entities;
using EchoCore.Domain.Repositories;
using EchoCore.Domain.Services;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.Linq;

namespace EchoCore.Infrastructure.Services;

public class MemorySystemClient : IMemorySystemClient
{
    private readonly IGptClient _gptClient;
    private readonly IMemoryRepository _memoryRepository;
    private readonly ISemanticMemoryRepository _semanticMemoryRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MemorySystemClient> _logger;

    public MemorySystemClient(
        IGptClient gptClient,
        IMemoryRepository memoryRepository,
        ISemanticMemoryRepository semanticMemoryRepository,
        IEmbeddingService embeddingService,
        IUnitOfWork unitOfWork,
        ILogger<MemorySystemClient> logger)
    {
        _gptClient = gptClient;
        _memoryRepository = memoryRepository;
        _semanticMemoryRepository = semanticMemoryRepository;
        _embeddingService = embeddingService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> ChatWithMemoryAsync(Guid threadId, string userInput, CancellationToken cancellationToken = default)
    {
        try
        {
            var thread = await _memoryRepository.GetThreadWithEntriesAsync(threadId, asNoTracking: true);
            if (thread == null)
            {
                _logger.LogWarning("Thread ID {ThreadId} not found.", threadId);
                throw new Exception($"Thread ID {threadId} not found.");
            }

            var inputEmbedding = await _embeddingService.GetEmbeddingAsync(userInput);

            var semanticMemories = await _semanticMemoryRepository
                .SearchSimilarAsync(threadId, inputEmbedding, topN: 3);

            var semanticMessages = semanticMemories
                .OrderBy(e => e.CreatedAt)
                .Select(e => new Message(Role.User, $"(recall) {e.Content}"))
                .ToList();

            var history = thread.Entries
                .OrderBy(e => e.CreatedAt)
                .TakeLast(10)
                .Select(e => new Message(e.Role == "assistant" ? Role.Assistant : Role.User, e.Content))
                .ToList();

            history.AddRange(semanticMessages);
            history.Add(new Message(Role.User, userInput));

            var result = await _gptClient.GetChatCompletionAsync(history, cancellationToken);

            await _unitOfWork.BeginTransactionAsync();

            thread.Entries.Add(new MemoryEntry
            {
                ThreadId = threadId,
                Role = "user",
                Content = userInput,
                CreatedAt = DateTime.UtcNow
            });

            thread.Entries.Add(new MemoryEntry
            {
                ThreadId = threadId,
                Role = "assistant",
                Content = result,
                CreatedAt = DateTime.UtcNow
            });

            await _semanticMemoryRepository.Add(new SemanticMemoryEntry
            {
                ThreadId = threadId,
                Content = userInput,
                Embedding = inputEmbedding,
                CreatedAt = DateTime.UtcNow
            });

            await _memoryRepository.SaveChangesAsync();
            await _semanticMemoryRepository.SaveChangesAsync();

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("ChatWithMemoryAsync succeeded for thread {ThreadId}", threadId);

            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "ChatWithMemoryAsync failed for thread {ThreadId}", threadId);
            throw;
        }
    }
}
