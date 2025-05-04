using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Domain.Utilities
{
    public interface IFeedbackPromptBuilder
    {
        string BuildFeedbackPrompt(string originalPrompt, string badOutput);
    }
}
