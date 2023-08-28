using FileTime.Core.Models;

namespace FileTime.Core.Serialization;

public class AbsolutePathSerializer
{
    public static SerializedAbsolutePath Serialize(AbsolutePath absolutePath) 
        => new()
        {
            PointInTime = absolutePath.PointInTime,
            Path = absolutePath.Path.Path,
            Type = absolutePath.Type
        };
}