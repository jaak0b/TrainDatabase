using System;
using Helper;
using Microsoft.Extensions.Logging;

namespace Core
{
  public class LogEventBus
  {
    public event EventHandler<MessageLoggedEventArgs>? OnMessageLogged = default!;

    public void Log(LogLevel level, string? message, Exception? exception)
    {
      OnMessageLogged?.Invoke(this, new(level, message, exception));
    }

    public void Log(LogLevel level, Exception? exception)
    {
      OnMessageLogged?.Invoke(this, new(level, exception));
    }

    public void Log(LogLevel level, string? message)
    {
      OnMessageLogged?.Invoke(this, new(level, message));
    }
  }
}