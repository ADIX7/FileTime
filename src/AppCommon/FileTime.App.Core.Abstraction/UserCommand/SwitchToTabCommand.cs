namespace FileTime.App.Core.UserCommand;

public class SwitchToTabCommand : IIdentifiableUserCommand
{
    private const string SwitchToTabBase = "switch_to_tab";
    public const string SwitchToTab1CommandName = SwitchToTabBase + "1";
    public const string SwitchToTab2CommandName = SwitchToTabBase + "2";
    public const string SwitchToTab3CommandName = SwitchToTabBase + "3";
    public const string SwitchToTab4CommandName = SwitchToTabBase + "4";
    public const string SwitchToTab5CommandName = SwitchToTabBase + "5";
    public const string SwitchToTab6CommandName = SwitchToTabBase + "6";
    public const string SwitchToTab7CommandName = SwitchToTabBase + "7";
    public const string SwitchToTab8CommandName = SwitchToTabBase + "8";
    public const string SwitchToLastTabCommandName = "switch_to_last_tab";
    
    public static SwitchToTabCommand SwitchToTab1 { get; } = new(1, SwitchToTab1CommandName);
    public static SwitchToTabCommand SwitchToTab2 { get; } = new(2, SwitchToTab2CommandName);
    public static SwitchToTabCommand SwitchToTab3 { get; } = new(3, SwitchToTab3CommandName);
    public static SwitchToTabCommand SwitchToTab4 { get; } = new(4, SwitchToTab4CommandName);
    public static SwitchToTabCommand SwitchToTab5 { get; } = new(5, SwitchToTab5CommandName);
    public static SwitchToTabCommand SwitchToTab6 { get; } = new(6, SwitchToTab6CommandName);
    public static SwitchToTabCommand SwitchToTab7 { get; } = new(7, SwitchToTab7CommandName);
    public static SwitchToTabCommand SwitchToTab8 { get; } = new(8, SwitchToTab8CommandName);
    public static SwitchToTabCommand SwitchToLastTab { get; } = new(-1, SwitchToLastTabCommandName);
    
    private SwitchToTabCommand(int tabNumber, string commandName)
    {
        TabNumber = tabNumber;
        UserCommandID = commandName;
    }

    public string UserCommandID { get; }
    public int TabNumber { get; }
}