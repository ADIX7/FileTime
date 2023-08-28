using System.Runtime.Serialization;
using FileTime.Core.Enums;
using FileTime.Core.Timeline;
using MessagePack;

namespace FileTime.Core.Serialization;

[MessagePackObject]
[DataContract]
public class SerializedAbsolutePath
{
    [Key(0)] [DataMember(Order = 0)] public required PointInTime PointInTime { get; set; }
    [Key(1)] [DataMember(Order = 1)] public required string Path { get; set; }
    [Key(2)] [DataMember(Order = 2)] public required AbsolutePathType Type { get; set; }
}