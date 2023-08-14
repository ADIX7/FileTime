using FileTime.App.CommandPalette.ViewModels;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Services;
using FileTime.Core.Interactions;

namespace FileTime.ConsoleUI.App;

public class RootViewModel : IRootViewModel
{
    public string UserName => Environment.UserName;
    public string MachineName => Environment.MachineName;
    public IPossibleCommandsViewModel PossibleCommands { get; }
    public IConsoleAppState AppState { get; }
    public ICommandPaletteViewModel CommandPalette { get; }
    public IDialogService DialogService { get; }
    public event Action<IInputElement>? FocusReadInputElement;

    public RootViewModel(
        IConsoleAppState appState,
        IPossibleCommandsViewModel possibleCommands,
        ICommandPaletteViewModel commandPalette,
        IDialogService dialogService)
    {
        AppState = appState;
        PossibleCommands = possibleCommands;
        CommandPalette = commandPalette;
        DialogService = dialogService;

        DialogService.ReadInput.PropertyChanged += (o, e) =>
        {
            if (e.PropertyName == nameof(DialogService.ReadInput.Value))
            {
                if (DialogService.ReadInput.Value is {Inputs.Count: > 0} readInputs)
                {
                    FocusReadInputElement?.Invoke(readInputs.Inputs[0]);
                }
            }
        };
    }
}