using FileTime.App.Core.UserCommand;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class CommandSchedulerUserCommandHandlerService : UserCommandHandlerServiceBase
{
    private readonly ICommandScheduler _commandScheduler;

    public CommandSchedulerUserCommandHandlerService(ICommandScheduler commandScheduler)
    {
        _commandScheduler = commandScheduler;
        AddCommandHandler(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<PauseCommandSchedulerCommand>(PauseCommandScheduler),
            new TypeUserCommandHandler<StartCommandSchedulerCommand>(StartCommandScheduler),
        });
    }

    private async Task PauseCommandScheduler()
        => await _commandScheduler.SetRunningEnabledAsync(false);

    private async Task StartCommandScheduler()
        => await _commandScheduler.SetRunningEnabledAsync(true);
}