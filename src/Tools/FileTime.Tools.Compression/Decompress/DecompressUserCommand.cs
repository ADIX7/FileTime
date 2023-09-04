using FileTime.App.Core.UserCommand;

namespace FileTime.Tools.Compression.Decompress;

public class DecompressUserCommand : IIdentifiableUserCommand
{
    public const string CommandName = "decompress";
    public static readonly DecompressUserCommand Instance = new();
    private DecompressUserCommand()
    {
    }

    public string UserCommandID => CommandName;
    public string Title => "Select for decompression";
}