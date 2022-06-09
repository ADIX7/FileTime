using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Delete;

public class DeleteCommand : IExecutableCommand
{
    public bool HardDelete { get; init; }
    public List<FullName> ItemsToDelete { get; } = new List<FullName>();

    public Task<CanCommandRun> CanRun(PointInTime currentTime)
    {
        throw new NotImplementedException();
    }

    public Task<PointInTime> SimulateCommand(PointInTime currentTime)
    {
        throw new NotImplementedException();
    }

    public Task Execute()
    {
        throw new NotImplementedException();
    }
}