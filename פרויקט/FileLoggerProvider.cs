using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace MyMiddleware
{
    /// <summary>
    /// מספק ILogger פשוט שכותב לוגים לקובץ טקסט באופן thread-safe.
    /// </summary>
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        public FileLoggerProvider(string filePath)
        {
            _filePath = filePath;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_filePath, _lock, categoryName);
        }

        public void Dispose() { }
    }

    internal class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly object _lock;
        private readonly string _categoryName;

        public FileLogger(string filePath, object lockObj, string categoryName)
        {
            _filePath = filePath;
            _lock = lockObj;
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string message = formatter(state, exception);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logMessage = $"[{timestamp}] [{logLevel,-12}] [{_categoryName}] {message}";

            if (exception != null)
            {
                logMessage += Environment.NewLine + exception;
            }

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_filePath, logMessage + Environment.NewLine);
                }
                catch
                {
                    // ignore write errors
                }
            }
        }
    }

    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string filePath)
        {
            builder.AddProvider(new FileLoggerProvider(filePath));
            return builder;
        }
    }
}
