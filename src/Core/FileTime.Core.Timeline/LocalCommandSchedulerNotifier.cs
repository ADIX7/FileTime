using FileTime.Core.Models;

namespace FileTime.Core.Timeline;

public class LocalCommandSchedulerNotifier : ICommandSchedulerNotifier
{
    private readonly ICommandScheduler _commandRunner;

    public LocalCommandSchedulerNotifier(ICommandScheduler commandRunner)
    {
        _commandRunner = commandRunner;
    }
    public Task RefreshContainer(FullName container)
    {
        _commandRunner.RefreshContainer(container);
        return  Task.CompletedTask;
    }
}