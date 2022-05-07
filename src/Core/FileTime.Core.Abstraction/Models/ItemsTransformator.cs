namespace FileTime.Core.Models;

public record ItemsTransformator(
    string Name,
    Func<IEnumerable<IItem>, Task<IEnumerable<IItem>>> Transformator
);