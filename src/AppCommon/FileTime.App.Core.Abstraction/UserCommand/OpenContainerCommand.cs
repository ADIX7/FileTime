using FileTime.Core.Models;

namespace FileTime.App.Core.UserCommand;

public class OpenContainerCommand : IUserCommand
{
    public AbsolutePath Path { get; }

    public OpenContainerCommand(AbsolutePath path)
    {
        Path = path;
    }
}