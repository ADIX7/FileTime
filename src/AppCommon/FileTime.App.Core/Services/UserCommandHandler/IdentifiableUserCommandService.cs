using FileTime.App.Core.UserCommand;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class IdentifiableUserCommandService : IIdentifiableUserCommandService
{
    private readonly Dictionary<string, Func<IIdentifiableUserCommand>> _identifiableUserCommands = new();

    public void AddIdentifiableUserCommandFactory(string identifier, Func<IIdentifiableUserCommand> commandFactory)
        => _identifiableUserCommands.Add(identifier, commandFactory);

    public IIdentifiableUserCommand GetCommand(string identifier)
    {
        if (!_identifiableUserCommands.ContainsKey(identifier))
            throw new IndexOutOfRangeException($"No command factory is registered for command {identifier}");

        return _identifiableUserCommands[identifier].Invoke();
    }
    
    public IReadOnlyCollection<string> GetCommandIdentifiers() => _identifiableUserCommands.Keys.ToList();
}