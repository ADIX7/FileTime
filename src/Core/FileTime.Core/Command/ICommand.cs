using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public interface ICommand
    {
        Task<CanCommandRun> CanRun(PointInTime startPoint);
        Task<PointInTime> SimulateCommand(PointInTime startPoint);
    }
}