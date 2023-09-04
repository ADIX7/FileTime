namespace FileTime.Core.Models;

public readonly struct ItemInitializationSettings
{
    public bool SkipChildInitialization { get; init; }
    public AbsolutePath? Parent { get; init; }
}