using FileTime.App.CommandPalette.ViewModels;
using FileTime.App.Core.ViewModels;

namespace FileTime.ConsoleUI.App;

public class RootViewModel : IRootViewModel
{
    public string UserName => Environment.UserName;
    public string MachineName => Environment.MachineName;
    public IPossibleCommandsViewModel PossibleCommands { get; }
    public IConsoleAppState AppState { get; }
    public ICommandPaletteViewModel CommandPalette { get; }

    public RootViewModel(
        IConsoleAppState appState,
        IPossibleCommandsViewModel possibleCommands,
        ICommandPaletteViewModel commandPalette)
    {
        AppState = appState;
        PossibleCommands = possibleCommands;
        CommandPalette = commandPalette;
    }
}