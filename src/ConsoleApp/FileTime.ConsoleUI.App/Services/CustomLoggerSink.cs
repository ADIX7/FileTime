using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;

namespace FileTime.ConsoleUI.App.Services;

public class CustomLoggerSink : ILogEventSink
{
    private readonly Lazy<IDialogService> _dialogService;
    
    public CustomLoggerSink(IServiceProvider serviceProvider)
    {
        _dialogService = new Lazy<IDialogService>(() => serviceProvider.GetRequiredService<IDialogService>());
    }
    
    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level >= LogEventLevel.Error)
        {
            var message = logEvent.RenderMessage();
            if (logEvent.Exception is not null)
                message += $" {logEvent.Exception.Message}";
            Debug.WriteLine(message);
            _dialogService.Value.ShowToastMessage(message);
        }
    }
}