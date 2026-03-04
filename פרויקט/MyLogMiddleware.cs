using System.Diagnostics;
using MyMiddleware.Models;
using MyMiddleware.Services;

namespace MyMiddleware;

/// <summary>
/// Middleware that logs all HTTP requests asynchronously
/// Captures: endpoint, method, execution time, status code, user info
/// </summary>
public class MyLogMiddleware
{
    // Delegate to continue the request to the next middleware
    private readonly RequestDelegate _next;
    // Logger for middleware-level issues
    private readonly ILogger _logger;
    // Background queue for async logging
    private readonly BackgroundLogQueue _logQueue;

    public MyLogMiddleware(RequestDelegate next, ILogger<MyLogMiddleware> logger, BackgroundLogQueue logQueue)
    {
        _next = next;
        _logger = logger;
        _logQueue = logQueue;
    }

    /// <summary>
    /// Process the request and queue it for async logging
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        // Capture original request details
        string method = context.Request.Method;
        string path = context.Request.Path;
        string controllerAction = ExtractControllerAction(path);
        
        var userId = context.User?.FindFirst("userId")?.Value ?? "unknown";
        var username = context.User?.FindFirst("username")?.Value ?? userId;

        // Start timing
        var startTime = DateTime.Now;
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        int statusCode = 0;
        try
        {
            // Execute the request through the pipeline
            await _next.Invoke(context);
            statusCode = context.Response.StatusCode;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError($"ERROR in {controllerAction} | User: {username} | Error: {ex.Message}");
            throw;
        }

        stopwatch.Stop();

        // Queue the log entry for background processing
        var logEntry = new LogEntry
        {
            StartTime = startTime,
            ControllerAction = $"{method} {controllerAction}",
            UserName = username,
            DurationMs = stopwatch.ElapsedMilliseconds
        };

        _logQueue.EnqueueLog(logEntry);

        // Also log based on status code level
        if (statusCode >= 500)
        {
            _logger.LogError(CreateLogMessage(method, path, username, statusCode, stopwatch.ElapsedMilliseconds));
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(CreateLogMessage(method, path, username, statusCode, stopwatch.ElapsedMilliseconds));
        }
        else
        {
            _logger.LogInformation(CreateLogMessage(method, path, username, statusCode, stopwatch.ElapsedMilliseconds));
        }
    }

    /// <summary>
    /// Extract controller and action name from the path
    /// </summary>
    private string ExtractControllerAction(string path)
    {
        // Remove leading slash and split by /
        var parts = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return $"{parts[0]}/{parts[1]}";
        }
        return path;
    }

    /// <summary>
    /// Create a formatted log message with full details
    /// </summary>
    private string CreateLogMessage(string method, string path, string username, int statusCode, long elapsedMs)
    {
        return $"ACTION: {method} {path} | " +
               $"User: {username} | " +
               $"Status: {statusCode} | " +
               $"Duration: {elapsedMs}ms";
    }
}

/// <summary>
/// Extension methods for adding the middleware to the pipeline
/// </summary>
public static partial class MiddlewareExtensions
{
    /// <summary>
    /// Add MyLogMiddleware to the application builder
    /// </summary>
    public static IApplicationBuilder UseMyLogMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MyLogMiddleware>();
    }
}

