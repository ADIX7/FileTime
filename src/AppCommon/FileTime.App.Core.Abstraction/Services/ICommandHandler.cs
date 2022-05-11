using FileTime.App.Core.Command;

namespace FileTime.App.Core.Services;

public interface ICommandHandler
{
    bool CanHandleCommand(Command.Command command);
    Task HandleCommandAsync(Command.Command command);
}