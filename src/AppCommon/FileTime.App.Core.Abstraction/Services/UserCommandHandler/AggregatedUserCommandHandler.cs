namespace FileTime.App.Core.Services.UserCommandHandler;

public abstract class AggregatedUserCommandHandler : IUserCommandHandler
{
    private readonly List<IUserCommandHandler> _userCommandHandlers = new();

    public bool CanHandleCommand(UserCommand.IUserCommand command) => _userCommandHandlers.Any(h => h.CanHandleCommand(command));

    public async Task HandleCommandAsync(UserCommand.IUserCommand command)
    {
        var handler = _userCommandHandlers.Find(h => h.CanHandleCommand(command));

        if (handler is null) return;
        await handler.HandleCommandAsync(command);
    }

    protected void AddCommandHandler(IUserCommandHandler userCommandHandler) => _userCommandHandlers.Add(userCommandHandler);

    protected void AddCommandHandler(IEnumerable<IUserCommandHandler> commandHandlers)
    {
        foreach (var userCommandHandler in commandHandlers)
        {
            AddCommandHandler(userCommandHandler);
        }
    }
}