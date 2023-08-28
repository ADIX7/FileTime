using System.Runtime.Serialization;
using FileTime.Core.Enums;
using MessagePack;

namespace FileTime.Core.Serialization.Container;

[DataContract]
[MessagePackObject]
public class SerializedContainer : ISerialized
{
    [Key(0)] [DataMember(Order = 0)] public required int Id { get; set; }
    [Key(1)] [DataMember(Order = 1)] public required string Name { get; set; }

    [Key(2)] [DataMember(Order = 2)] public required string DisplayName { get; set; }

    [Key(3)] [DataMember(Order = 3)] public required string FullName { get; set; }

    [Key(4)] [DataMember(Order = 4)] public required string NativePath { get; set; }
    [Key(5)] [DataMember(Order = 5)] public required string Parent { get; set; }
    [Key(6)] [DataMember(Order = 6)] public required bool IsHidden { get; set; }
    [Key(7)] [DataMember(Order = 7)] public required bool IsExists { get; set; }
    [Key(8)] [DataMember(Order = 8)] public required DateTime? CreatedAt { get; set; }
    [Key(9)] [DataMember(Order = 9)] public required DateTime? ModifiedAt { get; set; }
    [Key(10)] [DataMember(Order = 10)] public required SupportsDelete CanDelete { get; set; }
    [Key(11)] [DataMember(Order = 11)] public required bool CanRename { get; set; }
    [Key(12)] [DataMember(Order = 12)] public required string? Attributes { get; set; }
    [Key(13)] [DataMember(Order = 13)] public required bool AllowRecursiveDeletion { get; set; }
    [Key(14)] [DataMember(Order = 14)] public required SerializedAbsolutePath[] Items { get; set; }
}