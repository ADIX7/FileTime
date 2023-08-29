using FileTime.Core.Models;

namespace FileTime.App.Core.UserCommand;

public class NewTabCommand : IUserCommand
{
    public NewTabCommand(FullName? path)
    {
        Path = path;
    }

    public FullName? Path { get; }
    public bool Open { get; init; } = true;
}
public class IdentifiableNewTabCommand : NewTabCommand, IIdentifiableUserCommand
{
    public const string CommandName = "new_tab";
    
    public static IdentifiableNewTabCommand Instance { get; } = new();
    
    private IdentifiableNewTabCommand():base(null)
    {
        
    }
    public string UserCommandID => CommandName;
    public string Title => "New tab";
}