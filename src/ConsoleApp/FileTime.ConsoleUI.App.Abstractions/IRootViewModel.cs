using FileTime.App.CommandPalette.ViewModels;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.Timeline;
using FileTime.ConsoleUI.App.Services;
using FileTime.Core.Interactions;

namespace FileTime.ConsoleUI.App;

public interface IRootViewModel
{
    IConsoleAppState AppState { get; }
    IPossibleCommandsViewModel PossibleCommands { get; }
    string UserName { get; }
    string MachineName { get; }
    ICommandPaletteViewModel CommandPalette { get; }
    IDialogService DialogService { get; }
    ITimelineViewModel TimelineViewModel { get; }
    event Action<IInputElement>? FocusReadInputElement;
}