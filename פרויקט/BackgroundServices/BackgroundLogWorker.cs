using MyMiddleware.Services;
using Serilog;

namespace MyMiddleware.BackgroundServices;
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
                    // Wait for the next log entry to be queued (async, without polling)
                    var logEntry = await logQueue.DequeueAsync(stoppingToken);
                    if (logEntry == null)
                        continue;

                    var logMessage = FormatLogEntry(logEntry);
logger.LogInformation(logMessage);                }
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
    return $"ACTION: {logEntry.HttpMethod} {logEntry.Path} | " +
           $"Controller: {logEntry.ControllerAction} | " +
           $"User: {logEntry.UserName} | " +
           $"Status: {logEntry.StatusCode} | " +
           $"Duration: {logEntry.DurationMs}ms";
}
        
    }
