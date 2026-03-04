using MyMiddleware.Models;

namespace MyMiddleware.Interfaces
{
    public interface ILogQueue
    {
        // Add a log entry to the background queue
        void EnqueueLog(LogEntry logEntry);
    }
}
