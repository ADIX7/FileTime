namespace FileTime.Core.Models
{
    public record FullName(string Path)
    {
        public FullName? GetParent()
        {
            if (Path is null) return null;

            var pathParts = Path.TrimEnd(Constants.SeparatorChar).Split(Constants.SeparatorChar);
            return pathParts.Length switch
            {
                > 1 => new(string.Join(Constants.SeparatorChar, pathParts.SkipLast(1))),
                _ => null
            };
        }
    }
}