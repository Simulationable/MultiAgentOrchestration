using EchoCore.Domain.Utilities;

namespace EchoCore.Utilities
{
    public class FeedbackPromptBuilder : IFeedbackPromptBuilder
    {
        public string BuildFeedbackPrompt(string originalPrompt, string badOutput)
        {
            var issues = AnalyzeBadOutput(badOutput);

            string feedbackIntro = "The previous output had the following specific issues:";
            string combinedFeedback = issues.Any()
                ? "- " + string.Join("\n- ", issues)
                : "- General formatting inconsistencies or missing required sections.";

            string suggestion =
                "To improve, please regenerate the response with the following corrections:\n" +
                "✅ Start directly with the required sections, no extra introductions or summaries.\n" +
                "✅ Use plain raw text without markdown, code blocks, or decorative wrappers.\n" +
                "✅ Ensure all required sections are included, correctly labeled, and ordered.\n" +
                "✅ Keep the language concise, focused, and machine-ready.\n" +
                "✅ Avoid unnecessary disclaimers, apologies, or meta-comments.\n\n" +
                "Here is the original instruction to apply:";

            return
                $"{feedbackIntro}\n{combinedFeedback}\n\n{suggestion}\n\n{originalPrompt}";
        }

        private List<string> AnalyzeBadOutput(string badOutput)
        {
            var issues = new List<string>();

            if (string.IsNullOrWhiteSpace(badOutput))
                issues.Add("The response was empty or blank.");

            if (badOutput.Contains("```"))
                issues.Add("Included unnecessary markdown code blocks (``` markers).");

            if (badOutput.Length > 2000)
                issues.Add("Response was excessively long, possibly due to redundancy.");

            if (!badOutput.Contains("{") || !badOutput.Contains("}"))
                issues.Add("Expected structured formatting (like JSON or labeled sections) was missing.");

            if (badOutput.Contains("warning", StringComparison.OrdinalIgnoreCase) ||
                badOutput.Contains("notice", StringComparison.OrdinalIgnoreCase))
                issues.Add("Included irrelevant warnings or notices.");

            if (badOutput.Contains("I'm sorry", StringComparison.OrdinalIgnoreCase) ||
                badOutput.Contains("As an AI", StringComparison.OrdinalIgnoreCase))
                issues.Add("Included unnecessary disclaimers or apologies.");

            if (HasRepeatedLines(badOutput))
                issues.Add("Contained repeated or duplicate lines.");

            if (!IsLikelyValidJson(badOutput))
                issues.Add("Possible invalid JSON or unstructured formatting detected.");

            return issues;
        }

        private bool HasRepeatedLines(string text)
        {
            var lines = text.Split('\n')
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrEmpty(l))
                            .ToList();

            return lines.Count != lines.Distinct().Count();
        }

        private bool IsLikelyValidJson(string text)
        {
            try
            {
                System.Text.Json.JsonDocument.Parse(text);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
