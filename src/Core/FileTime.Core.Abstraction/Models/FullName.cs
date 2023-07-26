namespace FileTime.Core.Models;

public record FullName(string Path)
{
    public FullName? GetParent()
    {
        var pathParts = Path.TrimEnd(Constants.SeparatorChar).Split(Constants.SeparatorChar);
        return pathParts.Length switch
        {
            > 1 => CreateSafe(string.Join(Constants.SeparatorChar, pathParts.SkipLast(1))),
            _ => null
        };
    }

    public static FullName? CreateSafe(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;
        return new(path);
    }

    public string GetName()
        => Path.Split(Constants.SeparatorChar).Last();

    public FullName GetChild(string childName) 
        => new(Path + Constants.SeparatorChar + childName);

    public override string ToString() => Path;
}