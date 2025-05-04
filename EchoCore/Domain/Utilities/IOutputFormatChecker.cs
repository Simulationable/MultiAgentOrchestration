using Microsoft.AspNetCore.Mvc;

namespace EchoCore.Domain.Utilities
{
    public interface IOutputFormatChecker
    {
        bool IsValid(string result);
    }
}
