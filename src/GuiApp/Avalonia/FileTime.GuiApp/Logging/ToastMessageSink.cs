using FileTime.GuiApp.Services;
using Serilog.Core;
using Serilog.Events;

namespace FileTime.GuiApp.Logging;

public class ToastMessageSink : ILogEventSink
{
    private readonly IDialogService dialogService;

    public ToastMessageSink(IDialogService dialogService)
    {
        this.dialogService = dialogService;
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level >= LogEventLevel.Error)
        {
            var message = logEvent.RenderMessage();
            dialogService.ShowToastMessage(message);
        }
    }
}