using System.Reactive.Subjects;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.ViewModels.Timeline;

public class CommandTimeStateViewModel : ICommandTimeStateViewModel
{
    public IObservable<int> TotalProgress { get; }

    public IObservable<string> DisplayLabel { get; }

    public IObservable<bool> IsSelected { get; }

    public CommandTimeStateViewModel(CommandTimeState commandTimeState)
    {
        DisplayLabel = commandTimeState.Command.DisplayLabel;
        TotalProgress = commandTimeState.Command.TotalProgress;
        //TODO
        IsSelected = new BehaviorSubject<bool>(false);
    }
}