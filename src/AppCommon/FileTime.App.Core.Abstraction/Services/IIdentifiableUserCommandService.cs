using FileTime.App.Core.UserCommand;

namespace FileTime.App.Core.Services;

public interface IIdentifiableUserCommandService
{
    void AddIdentifiableUserCommandFactory(string identifier, Func<IIdentifiableUserCommand> commandFactory);
    IIdentifiableUserCommand GetCommand(string identifier);
    IReadOnlyCollection<string> GetCommandIdentifiers();
}