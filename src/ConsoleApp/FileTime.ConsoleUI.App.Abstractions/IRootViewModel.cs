using FileTime.App.Core.ViewModels;

namespace FileTime.ConsoleUI.App;

public interface IRootViewModel
{
    IConsoleAppState AppState { get; }
    IPossibleCommandsViewModel PossibleCommands { get; }
    string UserName { get; }
    string MachineName { get; }
}