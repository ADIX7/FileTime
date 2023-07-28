namespace FileTime.Core.Models;

public sealed class ItemInitializationSettings
{
    public bool SkipChildInitialization { get; init; }
    public AbsolutePath? Parent { get; init; }
}