using FileTime.GuiApp.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;

namespace FileTime.GuiApp.App.Logging;

public class ToastMessageSink : ILogEventSink
{
    private readonly Lazy<IDialogService> _dialogService;

    public ToastMessageSink(
        IServiceProvider serviceProvider)
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
            _dialogService.Value.ShowToastMessage(message);
        }
    }
}