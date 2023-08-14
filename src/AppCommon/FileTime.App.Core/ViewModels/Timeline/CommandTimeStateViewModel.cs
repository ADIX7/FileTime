using System.Reactive.Subjects;
using DeclarativeProperty;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.ViewModels.Timeline;

public class CommandTimeStateViewModel : ICommandTimeStateViewModel
{
    private readonly CommandTimeState _commandTimeState;
    public IDeclarativeProperty<int> TotalProgress { get; }
    public IDeclarativeProperty<int> CurrentProgress { get; }

    public IDeclarativeProperty<string> DisplayLabel { get; }
    public IDeclarativeProperty<string> DisplayDetailLabel { get; }

    public IDeclarativeProperty<bool> IsSelected { get; }

    public CommandTimeStateViewModel(CommandTimeState commandTimeState)
    {
        _commandTimeState = commandTimeState;
        DisplayLabel = commandTimeState.Command.DisplayLabel;
        DisplayDetailLabel = commandTimeState.Command.DisplayDetailLabel;
        TotalProgress = commandTimeState.Command.TotalProgress;
        CurrentProgress = commandTimeState.Command.CurrentProgress;
        //TODO
        IsSelected = new DeclarativeProperty<bool>(false);
    }

    public void Cancel()
        => _commandTimeState.Command.Cancel();
}