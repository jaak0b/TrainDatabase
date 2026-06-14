using Microsoft.Extensions.Logging;

namespace TrainDatabase.Core.Logging;

public class MessageLoggedEventArgs : EventArgs
{
    public MessageLoggedEventArgs(LogLevel logLevel, string? message, Exception? exception)
    {
        LogLevel = logLevel;
        Message = message;
        Exception = exception;
    }

    public MessageLoggedEventArgs(LogLevel logLevel, Exception? exception) : this(logLevel, null, exception)
    {
    }

    public MessageLoggedEventArgs(LogLevel logLevel, string? message) : this(logLevel, message, null)
    {
    }

    public DateTime DateTime { get; } = DateTime.Now;

    public string? Message { get; }

    public Exception? Exception { get; set; }

    public LogLevel LogLevel { get; }
}
