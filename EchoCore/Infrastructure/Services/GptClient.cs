using EchoCore.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.Text;
using System.Text.Json;

namespace EchoCore.Infrastructure.Services
{
    public class GptClient : IGptClient
    {
        private readonly OpenAIClient _client;
        private readonly HttpClient _http;
        private readonly ILogger<GptClient> _logger;
        private readonly string _apiKey;
        private readonly string _defaultModel;
        private readonly double _defaultTemperature;
        private readonly int _defaultMaxTokens;
        private readonly int _maxContextMessages;
        private readonly string _apiEndpoint;

        public GptClient(IConfiguration config, ILogger<GptClient> logger, IHttpClientFactory httpClientFactory)
        {
            _apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is missing.");
            _defaultModel = config["OpenAI:DefaultModel"] ?? "gpt-4o";
            _defaultTemperature = double.TryParse(config["OpenAI:DefaultTemperature"], out var temp) ? temp : 0.7;
            _defaultMaxTokens = int.TryParse(config["OpenAI:DefaultMaxTokens"], out var tokens) ? tokens : 4000;
            _maxContextMessages = int.TryParse(config["OpenAI:MaxContextMessages"], out var context) ? context : 10;
            _apiEndpoint = config["OpenAI:ApiEndpoint"] ?? "https://api.openai.com/v1/chat/completions";

            _client = new OpenAIClient(_apiKey);
            _http = httpClientFactory.CreateClient();
            _logger = logger;
        }


        public async Task<string> GetChatCompletionAsync(List<Message> messages, CancellationToken cancellationToken = default)
        {
            var response = await _client.ChatEndpoint.GetCompletionAsync(
                new ChatRequest(messages, model: _defaultModel, temperature: _defaultTemperature, maxTokens: _defaultMaxTokens),
                cancellationToken
            );

            return response.FirstChoice?.Message ?? string.Empty;
        }

        public async Task<string> GetChatCompletionAsync(List<Message> messages, string? model, double? temperature, CancellationToken cancellationToken = default)
        {
            var response = await _client.ChatEndpoint.GetCompletionAsync(
                new ChatRequest(messages, model: model, temperature: temperature, maxTokens: _defaultMaxTokens),
                cancellationToken
            );

            return response.FirstChoice?.Message ?? string.Empty;
        }

        public async Task<string> GetChatCompletionAsync(List<Message> messages, string? model, double? temperature, int? maxTokens, CancellationToken cancellationToken = default)
        {
            try
            {
                var trimmed = messages.TakeLast(_maxContextMessages).ToList();
                var response = await _client.ChatEndpoint.GetCompletionAsync(
                    new ChatRequest(trimmed, model: model ?? _defaultModel, temperature: temperature, maxTokens: maxTokens),
                    cancellationToken);

                var result = response.FirstChoice?.Message ?? string.Empty;
                _logger.LogInformation("GPT chat success: model={Model}, temp={Temp}, tokens={Tokens}", model, temperature, maxTokens);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatCompletionAsync: model={Model}, temp={Temp}, tokens={Tokens}", model, temperature, maxTokens);
                throw;
            }
        }

        public async Task<string> GetChatCompletionWithImageAsync(List<Message> priorMessages, string prompt, byte[] imageBytes, string model, int maxTokens, CancellationToken cancellationToken = default)
        {
            try
            {
                var base64 = Convert.ToBase64String(imageBytes);
                var dataUrl = $"data:image/png;base64,{base64}";

                var jsonMessages = priorMessages.Select(m => new
                {
                    role = m.Role == Role.Assistant ? "assistant" : "user",
                    content = m.Content
                }).ToList<object>();

                jsonMessages.Add(new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new { type = "image_url", image_url = new { url = dataUrl } }
                    }
                });

                var request = new { model = model ?? _defaultModel, messages = jsonMessages, max_tokens = maxTokens };
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                var req = new HttpRequestMessage(HttpMethod.Post, _apiEndpoint);
                req.Headers.Add("Authorization", $"Bearer {_apiKey}");
                req.Content = content;

                var res = await _http.SendAsync(req, cancellationToken);
                if (!res.IsSuccessStatusCode)
                {
                    var errorContent = await res.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("GPT image chat error: status={Status}, content={Content}", res.StatusCode, errorContent);
                    res.EnsureSuccessStatusCode();
                }

                var json = await res.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(json);

                var result = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;

                _logger.LogInformation("GPT image chat success: prompt length={Len}, image size={Size} bytes", prompt.Length, imageBytes.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChatCompletionWithImageAsync: prompt length={Len}, image size={Size}", prompt.Length, imageBytes.Length);
                throw;
            }
        }
    }
}
