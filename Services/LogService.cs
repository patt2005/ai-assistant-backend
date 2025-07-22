using System.Text.Json;
using QwenChatBackend.Models;

namespace QwenChatBackend.Services
{
    public class LogService : ILogService
    {
        private readonly ILogger<LogService> _logger;

        public LogService(ILogger<LogService> logger)
        {
            _logger = logger;
        }

        public async Task LogAsync(Log log)
        {
            string logJson = JsonSerializer.Serialize(log, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            _logger.LogInformation(logJson);

            await Task.CompletedTask;
        }
    }
}