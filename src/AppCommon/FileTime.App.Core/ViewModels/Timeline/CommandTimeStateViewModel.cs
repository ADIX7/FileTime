using System.Reactive.Subjects;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.ViewModels.Timeline;

public class CommandTimeStateViewModel : ICommandTimeStateViewModel
{
    private readonly CommandTimeState _commandTimeState;
    public IObservable<int> TotalProgress { get; }
    public IObservable<int> CurrentProgress { get; }

    public IObservable<string> DisplayLabel { get; }
    public IObservable<string> DisplayDetailLabel { get; }

    public IObservable<bool> IsSelected { get; }

    public CommandTimeStateViewModel(CommandTimeState commandTimeState)
    {
        _commandTimeState = commandTimeState;
        DisplayLabel = commandTimeState.Command.DisplayLabel;
        DisplayDetailLabel = commandTimeState.Command.DisplayDetailLabel;
        TotalProgress = commandTimeState.Command.TotalProgress;
        CurrentProgress = commandTimeState.Command.CurrentProgress;
        //TODO
        IsSelected = new BehaviorSubject<bool>(false);
    }

    public void Cancel()
        => _commandTimeState.Command.Cancel();
}