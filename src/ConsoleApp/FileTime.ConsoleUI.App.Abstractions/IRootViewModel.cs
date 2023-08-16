using DeclarativeProperty;
using FileTime.App.CommandPalette.ViewModels;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.Timeline;
using FileTime.ConsoleUI.App.Services;
using FileTime.Core.Interactions;
using FileTime.Core.Models;

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
    IDeclarativeProperty<VolumeSizeInfo?> VolumeSizeInfo { get; }
    event Action<IInputElement>? FocusReadInputElement;
}