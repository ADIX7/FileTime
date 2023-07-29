using FileTime.App.Core.UserCommand;

namespace FileTime.App.Core.Services;

public interface IIdentifiableUserCommandService
{
    void AddIdentifiableUserCommandFactory(string identifier, IIdentifiableUserCommand commandFactory);
    IIdentifiableUserCommand? GetCommand(string identifier);
    IReadOnlyCollection<string> GetCommandIdentifiers();
    IReadOnlyDictionary<string, IIdentifiableUserCommand> IdentifiableUserCommands { get; }
}