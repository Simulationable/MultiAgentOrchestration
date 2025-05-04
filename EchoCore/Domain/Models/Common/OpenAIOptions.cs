namespace EchoCore.Domain.Models.Common
{
    public class OpenAIOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ChatModel { get; set; } = "gpt-4o";
        public string EmbeddingModel { get; set; } = "text-embedding-3-small";
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 4000;
        public int MaxContextMessages { get; set; } = 10;
    }
}
