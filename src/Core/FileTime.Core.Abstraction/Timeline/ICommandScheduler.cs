using FileTime.Core.Command;

namespace FileTime.Core.Timeline;

public interface ICommandScheduler
{
    Task AddCommand(ICommand command, int? batchId = null, bool toNewBatch = false);
}