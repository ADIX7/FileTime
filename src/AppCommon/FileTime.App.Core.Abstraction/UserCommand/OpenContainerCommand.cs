using FileTime.Core.Models;

namespace FileTime.App.Core.UserCommand;

public class OpenContainerCommand : IUserCommand
{
    public IAbsolutePath Path { get; }

    public OpenContainerCommand(IAbsolutePath path)
    {
        Path = path;
    }
}