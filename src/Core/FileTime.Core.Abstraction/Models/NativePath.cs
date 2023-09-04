using System.Diagnostics;

namespace FileTime.Core.Models;

[DebuggerDisplay("{Path}")]
public record NativePath(string Path)
{
    public override string ToString() => Path;
}