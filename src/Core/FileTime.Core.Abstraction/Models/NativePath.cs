namespace FileTime.Core.Models;

public record NativePath(string Path)
{
    public override string ToString() => Path;
}