namespace FileTime.App.Core.UserCommand;

public class AddRemoteContentProviderCommand : IIdentifiableUserCommand
{
    public const string CommandName = "add_remote_content_provider";
    
    public static AddRemoteContentProviderCommand Instance { get; } = new();

    private AddRemoteContentProviderCommand()
    {
        
    }
    
    public string UserCommandID => CommandName;
    public string Title => "Add Remote Content Provider";
}