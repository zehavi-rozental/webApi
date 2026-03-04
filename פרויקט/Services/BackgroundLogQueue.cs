using System.Collections.Concurrent;
using MyMiddleware.Models;
using MyMiddleware.Interfaces;

namespace MyMiddleware.Services
{
    /// <summary>
    /// Thread-safe queue for logging entries to be processed asynchronously
    /// </summary>
    public class BackgroundLogQueue : ILogQueue
    {
        private readonly ConcurrentQueue<LogEntry> logQueue = new();
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public void EnqueueLog(LogEntry logEntry)
        {
            logQueue.Enqueue(logEntry);
        }

        public bool TryDequeueLog(out LogEntry? logEntry)
        {
            return logQueue.TryDequeue(out logEntry);
        }

        public CancellationToken GetCancellationToken()
        {
            return cancellationTokenSource.Token;
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
