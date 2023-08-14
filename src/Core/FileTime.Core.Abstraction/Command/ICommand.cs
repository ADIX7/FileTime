using DeclarativeProperty;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command;

public interface ICommand
{
    IDeclarativeProperty<string> DisplayLabel { get; }
    IDeclarativeProperty<string> DisplayDetailLabel { get; }
    IDeclarativeProperty<int> TotalProgress { get; }
    IDeclarativeProperty<int> CurrentProgress { get; }

    Task<CanCommandRun> CanRun(PointInTime currentTime);
    Task<PointInTime> SimulateCommand(PointInTime currentTime);
    void Cancel();
}