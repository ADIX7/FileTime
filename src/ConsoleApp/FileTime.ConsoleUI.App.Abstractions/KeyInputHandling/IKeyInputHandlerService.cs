using FileTime.App.Core.Models;

namespace FileTime.ConsoleUI.App.KeyInputHandling;

public interface IKeyInputHandlerService
{
    void HandleKeyInput(GeneralKeyEventArgs keyEvent);
}