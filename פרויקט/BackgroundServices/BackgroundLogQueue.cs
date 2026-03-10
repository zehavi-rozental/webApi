using System.Collections.Concurrent;
using MyMiddleware.Models;
using MyMiddleware.Interfaces;

namespace MyMiddleware.BackgroundServices;
    /// <summary>
    /// Thread-safe queue for logging entries to be processed asynchronously
    /// </summary>
    public class BackgroundLogQueue : ILogQueue
    {
        private readonly ConcurrentQueue<LogEntry> logQueue = new();
        private readonly SemaphoreSlim signal = new(0);
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public void EnqueueLog(LogEntry logEntry)
        {
            logQueue.Enqueue(logEntry);
            signal.Release();
        }

        public async Task<LogEntry?> DequeueAsync(CancellationToken cancellationToken)
        {
            try
            {
                await signal.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            logQueue.TryDequeue(out var logEntry);
            return logEntry;
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

