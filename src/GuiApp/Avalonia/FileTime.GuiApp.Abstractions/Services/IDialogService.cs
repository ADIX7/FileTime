using FileTime.App.Core.Models;
using FileTime.Core.Interactions;
using FileTime.GuiApp.ViewModels;

namespace FileTime.GuiApp.Services;

public interface IDialogService : IUserCommunicationService
{
    IObservable<ReadInputsViewModel?> ReadInput { get; }
    IObservable<MessageBoxViewModel?> LastMessageBox { get; }
    void ReadInputs(IEnumerable<IInputElement> inputs, Action inputHandler, Action? cancelHandler = null);
}