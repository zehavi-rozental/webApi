namespace MyMiddleware.Models
{
    public class LogEntry
    {
        public DateTime StartTime { get; set; }
        public string ControllerAction { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public long DurationMs { get; set; }
    }
}
