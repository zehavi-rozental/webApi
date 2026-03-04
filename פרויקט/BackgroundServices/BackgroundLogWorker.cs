using MyMiddleware.Services;
using Serilog;

namespace MyMiddleware.BackgroundServices
{
    /// <summary>
    /// Background service that processes logs from the queue asynchronously
    /// Writes to disk using Serilog with rolling file policies
    /// </summary>
    public class BackgroundLogWorker : BackgroundService
    {
        private readonly BackgroundLogQueue logQueue;
        private readonly ILogger<BackgroundLogWorker> logger;

        public BackgroundLogWorker(BackgroundLogQueue logQueue, ILogger<BackgroundLogWorker> logger)
        {
            this.logQueue = logQueue;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Background Log Worker started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Try to dequeue a log entry
                    if (logQueue.TryDequeueLog(out var logEntry))
                    {
                        // Write the log asynchronously
                        await Task.Run(() =>
                        {
                            if (logEntry != null)
                            {
                                string logMessage = FormatLogEntry(logEntry);
                                Log.Information(logMessage);
                            }
                        }, stoppingToken);
                    }
                    else
                    {
                        // No logs to process, wait a bit before checking again
                        await Task.Delay(100, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Background Log Worker was cancelled");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Background Log Worker");
            }
        }

        private string FormatLogEntry(MyMiddleware.Models.LogEntry logEntry)
        {
            return $"[{logEntry.StartTime:yyyy-MM-dd HH:mm:ss}] " +
                   $"Controller: {logEntry.ControllerAction} | " +
                   $"User: {logEntry.UserName} | " +
                   $"Duration: {logEntry.DurationMs}ms";
        }
    }
}
