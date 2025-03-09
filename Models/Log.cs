namespace QwenChatBackend.Models
{
    public class Log
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Method { get; set; }
        public string Endpoint { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
    }
}