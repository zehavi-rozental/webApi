using System.Diagnostics;

namespace MyMiddleware;

/// <summary>
/// // middleware אשר logs את כל בקשות ה-HTTP
/// // תופס: endpoint, method, זמן ביצוע, status code, user info
/// </summary>
public class MyLogMiddleware
{
    // // דלגט להמשך הבקשה ל-middleware הבא
    private readonly RequestDelegate _next;
    // // logger לכתיבה של לוגים
    private readonly ILogger _logger;

    public MyLogMiddleware(RequestDelegate next, ILogger<MyLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// // עיבוד בקשה ול logging של כל הפרטים
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        // // שמירת פרטי הבקשה המקורית
        string method = context.Request.Method;
        string path = context.Request.Path;
        string queryString = context.Request.QueryString.ToString();
        string userId = context.User?.FindFirst("userId")?.Value ?? "unknown";
        string userAgent = context.Request.Headers["User-Agent"].ToString();
        string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // // יצירת טיימר למדידת זמן הביצוע
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // // שמירת status code מקורי ו-stream למדידה
        int statusCode = 0;
        try
        {
            // // ביצוע הבקשה דרך middleware הבא
            await _next.Invoke(context);
            statusCode = context.Response.StatusCode;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            // // logging של שגיאה בביצוע בקשה
            _logger.LogError($"ERROR | Path: {path} | Method: {method} | User: {userId} | " +
                           $"IP: {ipAddress} | Time: {stopwatch.ElapsedMilliseconds}ms | " +
                           $"Message: {ex.Message}");
            throw;
        }

        stopwatch.Stop();

        // // קביעת רמת logging בהתאם ל-status code
        if (statusCode >= 500)
        {
            _logger.LogError(CreateLogMessage(method, path, userId, statusCode, stopwatch.ElapsedMilliseconds, queryString, ipAddress));
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(CreateLogMessage(method, path, userId, statusCode, stopwatch.ElapsedMilliseconds, queryString, ipAddress));
        }
        else
        {
            _logger.LogInformation(CreateLogMessage(method, path, userId, statusCode, stopwatch.ElapsedMilliseconds, queryString, ipAddress));
        }
    }

    /// <summary>
    /// // יצירת הודעת log עשירה עם כל הפרטים
    /// </summary>
    private string CreateLogMessage(string method, string path, string userId, int statusCode, long elapsedMs, string queryString, string ipAddress)
    {
        // // פורמט: פעולה | נתיב | משתמש | IP | Status | זמן
        return $"ACTION: {method} {path} | " +
               $"Query: {(string.IsNullOrEmpty(queryString) ? "none" : queryString)} | " +
               $"User: {userId} | " +
               $"IP: {ipAddress} | " +
               $"Status: {statusCode} | " +
               $"Duration: {elapsedMs}ms";
    }
}

/// <summary>
/// // הרחבות עבור הוספת ה-middleware לpipeline
/// </summary>
public static partial class MiddlewareExtensions
{
    /// <summary>
    /// // הוספת MyLogMiddleware ל-application builder
    /// </summary>
    public static IApplicationBuilder UseMyLogMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MyLogMiddleware>();
    }
}

