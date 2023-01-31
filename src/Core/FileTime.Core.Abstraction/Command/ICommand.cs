using FileTime.Core.Timeline;

namespace FileTime.Core.Command;

public interface ICommand
{
    IObservable<string> DisplayLabel { get; }
    IObservable<int> TotalProgress { get; }
    IObservable<int> CurrentProgress { get; }

    Task<CanCommandRun> CanRun(PointInTime currentTime);
    Task<PointInTime> SimulateCommand(PointInTime currentTime);
}