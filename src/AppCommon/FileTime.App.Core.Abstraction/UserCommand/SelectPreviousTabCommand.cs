namespace FileTime.App.Core.UserCommand;

public class SelectPreviousTabCommand : IIdentifiableUserCommand
{
    public const string CommandName = "previous_tab";
    
    public static SelectPreviousTabCommand Instance { get; } = new();
    
    private SelectPreviousTabCommand()
    {
        
    }
    public string UserCommandID => CommandName;
    public string Title => "New tab";
}