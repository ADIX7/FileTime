using FileTime.App.Core.UserCommand;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class IdentifiableUserCommandService : IIdentifiableUserCommandService
{
    private readonly Dictionary<string, IIdentifiableUserCommand> _identifiableUserCommands = new();
    public IReadOnlyDictionary<string, IIdentifiableUserCommand> IdentifiableUserCommands { get; }

    public IdentifiableUserCommandService()
    {
        IdentifiableUserCommands = _identifiableUserCommands.AsReadOnly();
    }

    public void AddIdentifiableUserCommand(IIdentifiableUserCommand command)
        => _identifiableUserCommands.Add(command.UserCommandID, command);

    public IIdentifiableUserCommand? GetCommand(string identifier)
    {
        //TODO: refactor to not throw an exception
        if (!_identifiableUserCommands.ContainsKey(identifier))
            throw new IndexOutOfRangeException($"No command factory is registered for command {identifier}");

        return _identifiableUserCommands[identifier];
    }
    
    public IReadOnlyCollection<string> GetCommandIdentifiers() => _identifiableUserCommands.Keys.ToList();
}