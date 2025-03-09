using QwenChatBackend.Models;

namespace QwenChatBackend.Services
{
    public interface ILogService
    {
        Task LogAsync(Log log);
    }
}