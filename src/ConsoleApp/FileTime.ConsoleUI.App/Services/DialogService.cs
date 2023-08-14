using FileTime.App.Core.Services;

namespace FileTime.ConsoleUI.App.Services;

public class DialogService : DialogServiceBase, IDialogService
{
    public DialogService(IModalService modalService) : base(modalService)
    {
    }

    public override void ShowToastMessage(string text)
    {
        // TODO: Implement
    }
}