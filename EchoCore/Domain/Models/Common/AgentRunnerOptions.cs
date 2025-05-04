namespace EchoCore.Domain.Models.Common
{
    public class AgentRunnerOptions
    {
        public int MaxRetries { get; set; } = 3;
        public int PageSize { get; set; } = 20;
        public int TopSimilarItems { get; set; } = 3;
    }

}
