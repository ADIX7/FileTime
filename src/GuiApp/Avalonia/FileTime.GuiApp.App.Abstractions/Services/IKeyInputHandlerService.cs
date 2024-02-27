using Avalonia.Input;

namespace FileTime.GuiApp.App.Services;

public interface IKeyInputHandlerService
{
    Task ProcessKeyDown(KeyEventArgs e);
    event EventHandler? UnhandledEsc;
}