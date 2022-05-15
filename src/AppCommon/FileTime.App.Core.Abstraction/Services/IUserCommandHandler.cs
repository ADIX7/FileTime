using FileTime.App.Core.UserCommand;

namespace FileTime.App.Core.Services;

public interface IUserCommandHandler
{
    bool CanHandleCommand(IUserCommand command);
    Task HandleCommandAsync(IUserCommand command);
}