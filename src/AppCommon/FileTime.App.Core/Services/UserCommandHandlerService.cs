using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Core.Services;

public class UserCommandHandlerService : IUserCommandHandlerService
{
    private readonly Lazy<IEnumerable<IUserCommandHandler>> _commandHandlers;

    public UserCommandHandlerService(IServiceProvider serviceProvider)
    {
        _commandHandlers = new Lazy<IEnumerable<IUserCommandHandler>>(serviceProvider.GetServices<IUserCommandHandler>);
    }

    public async Task HandleCommandAsync(UserCommand.IUserCommand command)
    {
        var handler = _commandHandlers.Value.FirstOrDefault(h => h.CanHandleCommand(command));

        if (handler != null)
        {
            await handler.HandleCommandAsync(command);
        }
    }

    public async Task HandleCommandAsync<TUserCommand>() where TUserCommand : UserCommand.IUserCommand, new()
        => await HandleCommandAsync(new TUserCommand());
}