using FileTime.Core.Timeline;

namespace FileTime.Core.Serialization.Timeline;

public class SerializedDifference
{
    public required SerializedAbsolutePath AbsolutePath { get; set; }
    public required DifferenceActionType Action { get; set; }
}