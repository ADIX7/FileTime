namespace FileTime.App.Core.UserCommand;

public class SelectNextTabCommand : IIdentifiableUserCommand
{
    public const string CommandName = "next_tab";
    
    public static SelectNextTabCommand Instance { get; } = new();
    
    private SelectNextTabCommand()
    {
        
    }
    public string UserCommandID => CommandName;
    public string Title => "Next tab";
}