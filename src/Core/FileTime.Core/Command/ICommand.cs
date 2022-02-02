using AsyncEvent;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public interface ICommand
    {
        string DisplayLabel { get; }
        Task<CanCommandRun> CanRun(PointInTime startPoint);
        Task<PointInTime> SimulateCommand(PointInTime startPoint);
        int Progress { get; }
        AsyncEventHandler ProgressChanged { get; }
    }
}