using FileTime.Core.Models;
using FileTime.Core.Services;

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