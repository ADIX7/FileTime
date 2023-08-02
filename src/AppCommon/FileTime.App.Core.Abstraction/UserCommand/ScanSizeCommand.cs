namespace FileTime.App.Core.UserCommand;

public sealed class ScanSizeCommand : IIdentifiableUserCommand
{
    public const string ScanSizeCommandName = "scan_size";
    public static readonly ScanSizeCommand Instance = new();

    private ScanSizeCommand()
    {
    }

    public string UserCommandID => ScanSizeCommandName;
    public string Title => "Scan size";
}