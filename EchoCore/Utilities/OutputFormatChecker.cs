using EchoCore.Domain.Utilities;

namespace EchoCore.Utilities
{
    public class OutputFormatChecker : IOutputFormatChecker
    {
        public bool IsValid(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return false;

            if (output.Contains("Here is") || output.Contains("Explanation:"))
                return false;

            return true;
        }
    }
}
