using Avalonia.Input;

namespace FileTime.GuiApp.App.Services;

public interface IKeyInputHandlerService
{
    Task ProcessKeyDown(Key key, KeyModifiers keyModifiers, Action<bool> setHandled);
}