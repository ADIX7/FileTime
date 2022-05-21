using FileTime.Core.Timeline;

namespace FileTime.Core.Command.CreateContainer;

public class CreateContainerCommand : IExecutableCommand
{
    public Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        throw new NotImplementedException();
    }

    public Task<PointInTime?> SimulateCommand(PointInTime? currentTime)
    {
        throw new NotImplementedException();
    }

    public Task Execute(ICommandScheduler commandScheduler)
    {
        throw new NotImplementedException();
    }
}