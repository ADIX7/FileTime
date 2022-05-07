using FileTime.App.Core.Command;

namespace FileTime.App.Core.Services;

public interface ICommandHandler
{
    bool CanHandleCommand(Commands command);
    Task HandleCommandAsync(Commands command);
}