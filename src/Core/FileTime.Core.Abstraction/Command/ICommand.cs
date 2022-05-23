using FileTime.Core.Timeline;

namespace FileTime.Core.Command;

public interface ICommand
{
    Task<CanCommandRun> CanRun(PointInTime currentTime);
    Task<PointInTime> SimulateCommand(PointInTime currentTime);
}