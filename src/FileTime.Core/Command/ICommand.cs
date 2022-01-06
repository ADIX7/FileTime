using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public interface ICommand
    {
        PointInTime SimulateCommand(PointInTime moment);
    }
}