using FileTime.App.Core.Services;

namespace FileTime.ConsoleUI.App.Services;

public class DialogService : DialogServiceBase, IDialogService
{
    private readonly IConsoleAppState _consoleAppState;

    public DialogService(IModalService modalService, IConsoleAppState consoleAppState) : base(modalService)
    {
        _consoleAppState = consoleAppState;
    }


    public override void ShowToastMessage(string text)
        => Task.Run(async () =>
        {
            _consoleAppState.PopupTexts.Add(text);
            await Task.Delay(5000);
            _consoleAppState.PopupTexts.Remove(text);
        });
}