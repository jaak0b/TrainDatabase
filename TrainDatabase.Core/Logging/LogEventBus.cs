using Microsoft.Extensions.Logging;

namespace TrainDatabase.Core.Logging;

/// <summary>In-app log event aggregator used to surface log messages in the UI.</summary>
public class LogEventBus
{
    public event EventHandler<MessageLoggedEventArgs>? OnMessageLogged;

    public void Log(LogLevel level, string? message, Exception? exception) =>
        OnMessageLogged?.Invoke(this, new MessageLoggedEventArgs(level, message, exception));

    public void Log(LogLevel level, Exception? exception) =>
        OnMessageLogged?.Invoke(this, new MessageLoggedEventArgs(level, exception));

    public void Log(LogLevel level, string? message) =>
        OnMessageLogged?.Invoke(this, new MessageLoggedEventArgs(level, message));
}
