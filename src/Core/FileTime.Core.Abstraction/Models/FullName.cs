namespace FileTime.Core.Models;

public record FullName(string Path)
{
    public FullName? GetParent()
    {
        if (Path.Length == 0) return null;
        var pathParts = Path.TrimEnd(Constants.SeparatorChar).Split(Constants.SeparatorChar);
        return pathParts.Length switch
        {
            > 1 => CreateSafe(string.Join(Constants.SeparatorChar, pathParts.SkipLast(1))),
            _ => CreateSafe("")
        };
    }

    public static FullName? CreateSafe(string? path)
    {
        if (path is null)
            return null;
        if (string.IsNullOrWhiteSpace(path))
            return new FullName("");

        return new(path);
    }

    public string GetName()
        => Path.Split(Constants.SeparatorChar).Last();

    public FullName GetChild(string childName)
        => new(Path + Constants.SeparatorChar + childName);

    public override string ToString() => Path;
}