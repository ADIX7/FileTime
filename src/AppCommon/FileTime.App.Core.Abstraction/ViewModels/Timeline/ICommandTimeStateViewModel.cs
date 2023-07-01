namespace FileTime.App.Core.ViewModels.Timeline;

public interface ICommandTimeStateViewModel
{
    IObservable<int> TotalProgress { get; }
    IObservable<int> CurrentProgress { get; }
    IObservable<string> DisplayLabel { get; }
    IObservable<bool> IsSelected { get; }
}