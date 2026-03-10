using System.Diagnostics;
using MyMiddleware.Models;
using MyMiddleware.Services;
using MyMiddleware.BackgroundServices;

namespace MyMiddleware;

public class MyLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly BackgroundLogQueue _logQueue;

    public MyLogMiddleware(RequestDelegate next, ILogger<MyLogMiddleware> logger, BackgroundLogQueue logQueue)
    {
        _next = next;
        _logger = logger;
        _logQueue = logQueue;
    }

    public async Task Invoke(HttpContext context)
    {
        // 1. איסוף נתונים התחלתיים
        string method = context.Request.Method;
        string path = context.Request.Path;
        string controllerAction = ExtractControllerAction(context);
        
        // שליפת שם משתמש מה-Token (Claims)
        var username = context.User?.FindFirst("username")?.Value 
                       ?? context.User?.FindFirst("userId")?.Value 
                       ?? "Guest";

        var startTime = DateTime.Now;
        var stopwatch = Stopwatch.StartNew();

        int statusCode = 0;

        try
        {
            // 2. המשך הצינור (Pipeline)
            await _next.Invoke(context);
            statusCode = context.Response.StatusCode;
        }
        catch (Exception)
        {
            statusCode = 500;
            throw; // חשוב לזרוק את השגיאה הלאה
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;

            // 1. יצירת אובייקט הלוג (הנתונים שהמורה ביקשה)
            var logEntry = new LogEntry
            {
                StartTime = startTime,
                HttpMethod = method,
                Path = path,
                ControllerAction = controllerAction,
                StatusCode = statusCode,
                UserName = username,
                DurationMs = duration
            };

            // 2. שליחה לתור האסינכרוני - זה מה שכותב לקובץ בסוף!
            _logQueue.EnqueueLog(logEntry);

        }
    }

    private string ExtractControllerAction(HttpContext context)
    {
        var routeData = context.GetRouteData();
        if (routeData?.Values != null && routeData.Values.TryGetValue("controller", out var controller))
        {
            routeData.Values.TryGetValue("action", out var action);
            return $"{controller}/{action}";
        }
        return context.Request.Path;
    }

    private string CreateLogMessage(string method, string path, string user, int status, long ms)
    {
        return $"[REQ] {method} {path} | User: {user} | Status: {status} | Duration: {ms}ms";
    }
}

public static partial class MiddlewareExtensions
{
    public static IApplicationBuilder UseMyLogMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MyLogMiddleware>();
    }
}