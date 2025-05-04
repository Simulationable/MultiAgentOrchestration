using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EchoCore.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EchoCore.Application.Services
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _http;
        private readonly ILogger<EmbeddingService> _logger;
        private readonly string _apiKey;
        private readonly string _embeddingModel;
        private readonly string _embeddingEndpoint;

        public EmbeddingService(IConfiguration config, ILogger<EmbeddingService> logger, IHttpClientFactory httpClientFactory)
        {
            _apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey missing in configuration.");
            _embeddingModel = config["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
            _embeddingEndpoint = config["OpenAI:EmbeddingEndpoint"] ?? "https://api.openai.com/v1/embeddings";

            _http = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestPayload = new
                {
                    input,
                    model = _embeddingModel
                };

                var json = JsonSerializer.Serialize(requestPayload);
                var message = new HttpRequestMessage(HttpMethod.Post, _embeddingEndpoint);
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                message.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.SendAsync(message, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Embedding API failed: {StatusCode} {Content}", response.StatusCode, errorContent);
                    throw new Exception($"Embedding API failed with status {response.StatusCode}: {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content, new JsonDocumentOptions { AllowTrailingCommas = true });

                var embedding = doc.RootElement
                                   .GetProperty("data")[0]
                                   .GetProperty("embedding")
                                   .EnumerateArray()
                                   .Select(e => e.GetSingle())
                                   .ToArray();

                _logger.LogInformation("Embedding retrieved successfully. Input length={Length}, Vector size={VectorSize}", input.Length, embedding.Length);
                return embedding;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching embedding for input length={Length}", input.Length);
                throw;
            }
        }
    }
}
