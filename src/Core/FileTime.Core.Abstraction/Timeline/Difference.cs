using FileTime.Core.Models;

namespace FileTime.Core.Timeline;

public class Difference
{
    public AbsolutePath AbsolutePath { get; }
    public DifferenceActionType Action { get; }

    public Difference(DifferenceActionType action, AbsolutePath absolutePath)
    {
        AbsolutePath = absolutePath;
        Action = action;
    }
}