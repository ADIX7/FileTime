using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace FileTime.ConsoleUI.App.Services;

public class CustomLoggerSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level >= LogEventLevel.Error)
        {
            var message = logEvent.RenderMessage();
            if (logEvent.Exception is not null)
                message += $" {logEvent.Exception.Message}";
            Debug.WriteLine(message);
        }
    }
}