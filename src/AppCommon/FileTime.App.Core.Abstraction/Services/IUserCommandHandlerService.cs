namespace FileTime.App.Core.Services;

public interface IUserCommandHandlerService
{
    Task HandleCommandAsync(UserCommand.IUserCommand command);
    Task HandleCommandAsync<TCommand>() where TCommand : UserCommand.IUserCommand, new();
}