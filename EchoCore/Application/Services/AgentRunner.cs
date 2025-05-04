using EchoCore.Domain.Entities;
using EchoCore.Domain.Models.Common;
using EchoCore.Domain.Models.Request;
using EchoCore.Domain.Repositories;
using EchoCore.Domain.Services;
using EchoCore.Domain.Utilities;
using EchoCore.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

public class AgentRunner : IAgentRunner
{
    private readonly IMemoryRepository _memoryRepo;
    private readonly IGptClient _gpt;
    private readonly IEmbeddingService _embedding;
    private readonly ISemanticMemoryRepository _semanticRepo;
    private readonly IPromptProfileService _promptProfile;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AgentRunner> _logger;
    private readonly OpenAIOptions _openAIOptions;
    private readonly AgentRunnerOptions _agentOptions;
    private readonly IFeedbackPromptBuilder _feedbackPromptBuilder;
    private readonly IOutputFormatChecker _outputFormatChecker;

    public AgentRunner(
        IMemoryRepository memoryRepo,
        IGptClient gpt,
        IEmbeddingService embedding,
        ISemanticMemoryRepository semanticRepo,
        IPromptProfileService promptProfile,
        IUnitOfWork unitOfWork,
        ILogger<AgentRunner> logger,
        IOptions<OpenAIOptions> openAIOptions,
        IOptions<AgentRunnerOptions> agentOptions,
        IFeedbackPromptBuilder feedbackPromptBuilder,
        IOutputFormatChecker outputFormatChecker)
    {
        _memoryRepo = memoryRepo;
        _gpt = gpt;
        _embedding = embedding;
        _semanticRepo = semanticRepo;
        _promptProfile = promptProfile;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _openAIOptions = openAIOptions.Value;
        _agentOptions = agentOptions.Value;
        _feedbackPromptBuilder = feedbackPromptBuilder;
        _outputFormatChecker = outputFormatChecker;
    }

    public async Task<string> RunAsync(AgentRunRequest request, CancellationToken cancellationToken = default)
    {
        int attempt = 0;
        string? result = null;
        string currentPrompt = request.Prompt;

        while (attempt < _agentOptions.MaxRetries)
        {
            attempt++;
            try
            {
                var thread = await _memoryRepo.GetThreadWithEntriesAsync(request.ThreadId, asNoTracking: true, cancellationToken);
                if (thread == null) throw new InvalidOperationException($"Thread {request.ThreadId} not found");

                var embedding = await _embedding.GetEmbeddingAsync(currentPrompt, cancellationToken);
                var memory = await _semanticRepo.SearchSimilarAsync(request.ThreadId, embedding, _agentOptions.TopSimilarItems, cancellationToken);
                var memoryBlock = string.Empty;

                if (memory != null && memory.Any())
                {
                    memoryBlock = string.Join("\n", memory.Select(m => $"- {m.Content}"));
                }
                else
                {
                    _logger.LogWarning("No memory entries found for thread {ThreadId}", request.ThreadId);
                    memoryBlock = "No relevant memory found.";
                }

                var template = await _promptProfile.GetPromptTemplateAsync(request.AgentType, cancellationToken);
                string finalPrompt;
                bool hasTemplate = !string.IsNullOrEmpty(template);

                if (hasTemplate)
                {
                    finalPrompt = template
                        .Replace("{memory}", memoryBlock)
                        .Replace("{prompt}", currentPrompt);
                }
                else
                {
                    finalPrompt = $"User prompt: {request.Prompt}\nRelevant memory:\n{memoryBlock}";
                }

                var messages = new List<Message>();

                if (!string.IsNullOrWhiteSpace(request.SystemMessage))
                {
                    messages.Add(new Message(Role.System, request.SystemMessage));
                }

                messages.Add(new Message(Role.User, finalPrompt));

                _logger.LogInformation("Sending prompt to GPT. Attempt={Attempt}, Model={Model}", attempt, request.Model);

                result = await _gpt.GetChatCompletionAsync(
                    messages,
                    request.Model,
                    request.Temperature,
                    request.MaxTokens,
                    cancellationToken
                );

                if (_outputFormatChecker.IsValid(result))
                {
                    _logger.LogInformation("Received valid response. Output size={Size} chars", result.Length);
                    break;
                }
                else
                {
                    _logger.LogWarning("Attempt {Attempt}: Invalid output format, retrying...", attempt);
                    currentPrompt = _feedbackPromptBuilder.BuildFeedbackPrompt(currentPrompt, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RunAsync attempt {Attempt}", attempt);
                if (attempt >= _agentOptions.MaxRetries) throw;
            }
        }

        if (result == null || !_outputFormatChecker.IsValid(result))
        {
            _logger.LogError("Failed to generate valid output after {MaxRetries} retries", _agentOptions.MaxRetries);
            throw new InvalidOperationException("Failed to generate valid output after retries");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await SaveMemoryEntriesAsync(request, currentPrompt, result, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Transaction committed for thread {ThreadId}", request.ThreadId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Transaction rolled back for thread {ThreadId}", request.ThreadId);
            throw;
        }

        return result;
    }

    public async Task<string> RunWithImageAsync(AgentRunRequest request, byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        try
        {
            var thread = await _memoryRepo.GetThreadWithEntriesAsync(request.ThreadId, asNoTracking: true, cancellationToken);
            if (thread == null) throw new InvalidOperationException($"Thread {request.ThreadId} not found");

            var embedding = await _embedding.GetEmbeddingAsync(request.Prompt, cancellationToken);
            var memory = await _semanticRepo.SearchSimilarAsync(request.ThreadId, embedding, _agentOptions.TopSimilarItems, cancellationToken);
            var memoryBlock = string.Join("\n", memory.Select(m => $"- {m.Content}"));

            var template = await _promptProfile.GetPromptTemplateAsync(request.AgentType, cancellationToken);
            var finalPrompt = template
                .Replace("{memory}", memoryBlock)
                .Replace("{prompt}", request.Prompt);

            var priorMessages = new List<Message>();
            if (!string.IsNullOrWhiteSpace(request.SystemMessage))
                priorMessages.Add(new Message(Role.System, request.SystemMessage));

            _logger.LogInformation("Sending image prompt to GPT.");

            var model = string.IsNullOrWhiteSpace(request.Model) ? "gpt-4o" : request.Model;
            var maxTokens = request.MaxTokens ?? 8000;

            var result = await _gpt.GetChatCompletionWithImageAsync(priorMessages, finalPrompt, imageBytes, model, maxTokens, cancellationToken);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            await _memoryRepo.AddEntryAsync(new MemoryEntry
            {
                ThreadId = request.ThreadId,
                Role = "user",
                Content = "[Image + Prompt] " + request.Prompt
            }, cancellationToken);

            await _memoryRepo.AddEntryAsync(new MemoryEntry
            {
                ThreadId = request.ThreadId,
                Role = "assistant",
                Content = result
            }, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Transaction committed for image-based run on thread {ThreadId}", request.ThreadId);

            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Transaction rolled back for image-based run on thread {ThreadId}", request.ThreadId);
            throw;
        }
    }

    public async Task<IEnumerable<MemoryEntry>> GetHistoryAsync(Guid threadId, int page, CancellationToken cancellationToken = default)
    {
        var thread = await _memoryRepo.GetThreadWithEntriesAsync(threadId, asNoTracking: true, cancellationToken);
        if (thread == null)
        {
            _logger.LogWarning("Thread {ThreadId} not found when fetching history", threadId);
            throw new InvalidOperationException("Thread not found");
        }

        var entries = thread.Entries
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * _agentOptions.PageSize)
            .Take(_agentOptions.PageSize)
            .Select(e => new MemoryEntry
            {
                Id = e.Id,
                ThreadId = e.ThreadId,
                Role = e.Role,
                Content = e.Content,
                CreatedAt = e.CreatedAt
            })
            .ToList();

        _logger.LogInformation("{Message}", $"Fetched {entries.Count} history entries for thread {threadId}");
        return entries;
    }

    private async Task SaveMemoryEntriesAsync(AgentRunRequest request, string userInput, string assistantOutput, CancellationToken cancellationToken = default)
    {
        await _memoryRepo.AddEntryAsync(new MemoryEntry
        {
            ThreadId = request.ThreadId,
            Role = "user",
            Content = userInput
        }, cancellationToken);

        await _memoryRepo.AddEntryAsync(new MemoryEntry
        {
            ThreadId = request.ThreadId,
            Role = "assistant",
            Content = assistantOutput
        }, cancellationToken);

        await _semanticRepo.Add(new SemanticMemoryEntry
        {
            Id = Guid.NewGuid(),
            ThreadId = request.ThreadId,
            Content = assistantOutput,
            Embedding = await _embedding.GetEmbeddingAsync(assistantOutput, cancellationToken)
        }, cancellationToken);

        await _memoryRepo.SaveChangesAsync(cancellationToken);
    }
}
