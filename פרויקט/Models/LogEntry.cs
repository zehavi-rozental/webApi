namespace MyMiddleware.Models
{
    public class LogEntry
    {
        public DateTime StartTime { get; set; }
        public string HttpMethod { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string ControllerAction { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string UserName { get; set; } = string.Empty;
        public long DurationMs { get; set; }
    }
}
