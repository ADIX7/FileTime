namespace FileTime.Core.Models;

public record FullName(string Path)
{
    public FullName? GetParent()
    {
        var pathParts = Path.TrimEnd(Constants.SeparatorChar).Split(Constants.SeparatorChar);
        return pathParts.Length switch
        {
            > 1 => new(string.Join(Constants.SeparatorChar, pathParts.SkipLast(1))),
            _ => null
        };
    }

    public string GetName()
        => Path.Split(Constants.SeparatorChar).Last();

    public FullName GetChild(string childName)
        => new FullName(Path + Constants.SeparatorChar + childName);
}