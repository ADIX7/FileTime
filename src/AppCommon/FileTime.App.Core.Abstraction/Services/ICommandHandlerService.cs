using FileTime.App.Core.Command;

namespace FileTime.App.Core.Services;

public interface ICommandHandlerService
{
    Task HandleCommandAsync(Command.Command command);
}