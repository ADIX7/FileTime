using FileTime.App.Core.UserCommand;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class TypeUserCommandHandler<T> : IUserCommandHandler
{
    private readonly Func<T, Task> _handler;

    public TypeUserCommandHandler(Func<T, Task> handler)
    {
        _handler = handler;
    }

    public TypeUserCommandHandler(Func<Task> handler)
    {
        _handler = async (_) => await handler();
    }

    public bool CanHandleCommand(IUserCommand command) => command is T;

    public Task HandleCommandAsync(IUserCommand command)
    {
        if (command is not T typedCommand) throw new ArgumentException($"Parameter '{nameof(command)}' is not of type '{typeof(T).Name}'");
        return _handler(typedCommand);
    }
}