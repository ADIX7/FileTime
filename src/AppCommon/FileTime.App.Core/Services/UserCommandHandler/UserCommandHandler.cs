namespace FileTime.App.Core.Services.UserCommandHandler;

public sealed class UserCommandHandler : IUserCommandHandler
{
    private readonly Func<UserCommand.IUserCommand, bool> _canHandle;
    private readonly Func<UserCommand.IUserCommand, Task> _handle;

    public UserCommandHandler(Func<UserCommand.IUserCommand, bool> canHandle, Func<UserCommand.IUserCommand, Task> handle)
    {
        _canHandle = canHandle;
        _handle = handle;
    }

    public bool CanHandleCommand(UserCommand.IUserCommand command) => _canHandle(command);
    public async Task HandleCommandAsync(UserCommand.IUserCommand command) => await _handle(command);
}