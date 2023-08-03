using FileTime.Core.Interactions;
using FileTime.GuiApp.App.ViewModels;

namespace FileTime.GuiApp.App.Services;

public interface IDialogService : IUserCommunicationService
{
    IObservable<ReadInputsViewModel?> ReadInput { get; }
    IObservable<MessageBoxViewModel?> LastMessageBox { get; }
}