using FileTime.Core.Models;

namespace FileTime.App.Core.UserCommand;

public sealed class OpenContainerCommand : IUserCommand
{
    public AbsolutePath Path { get; }

    public OpenContainerCommand(AbsolutePath path)
    {
        Path = path;
    }
}