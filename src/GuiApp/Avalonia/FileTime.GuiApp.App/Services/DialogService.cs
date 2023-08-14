using Avalonia.Threading;
using FileTime.App.Core.Services;
using FileTime.GuiApp.App.ViewModels;

namespace FileTime.GuiApp.App.Services;

public class DialogService : DialogServiceBase, IDialogService
{
    private readonly IGuiAppState _guiAppState;

    public DialogService(IModalService modalService, IGuiAppState guiAppState) : base(modalService)
    {
        _guiAppState = guiAppState;
    }

    public override void ShowToastMessage(string text)
        =>
            Task.Run(async () =>
            {
                await Dispatcher.UIThread.InvokeAsync(() => _guiAppState.PopupTexts.Add(text));
                await Task.Delay(5000);
                await Dispatcher.UIThread.InvokeAsync(() => _guiAppState.PopupTexts.Remove(text));
            });
}