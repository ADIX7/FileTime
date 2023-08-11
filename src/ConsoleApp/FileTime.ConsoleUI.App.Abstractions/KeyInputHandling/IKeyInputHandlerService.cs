using FileTime.App.Core.Models;
using GeneralInputKey;

namespace FileTime.ConsoleUI.App.KeyInputHandling;

public interface IKeyInputHandlerService
{
    void HandleKeyInput(GeneralKeyEventArgs keyEvent, SpecialKeysStatus specialKeysStatus);
}