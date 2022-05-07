using Avalonia.Input;
using FileTime.GuiApp.Models;

namespace FileTime.GuiApp.Services;

public interface IKeyInputHandler
{
    Task HandleInputKey(Key key, SpecialKeysStatus specialKeysStatus, Action<bool> setHandled);
}