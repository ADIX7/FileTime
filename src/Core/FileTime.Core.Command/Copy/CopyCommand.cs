using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Copy;

public class CopyCommand : ITransportationCommand
{
    public Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        throw new NotImplementedException();
    }

    public Task<PointInTime> SimulateCommand(PointInTime currentTime)
    {
        throw new NotImplementedException();
    }
}