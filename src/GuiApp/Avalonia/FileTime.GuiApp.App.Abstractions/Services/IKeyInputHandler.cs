using Avalonia.Input;
using FileTime.GuiApp.App.Models;

namespace FileTime.GuiApp.App.Services;

public interface IKeyInputHandler
{
    Task HandleInputKey(Key key, SpecialKeysStatus specialKeysStatus, Action<bool> setHandled);
}