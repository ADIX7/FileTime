namespace FileTime.Core.Models;

public record ItemFilter(
    string Name,
    Func<IItem, bool> Filter
);