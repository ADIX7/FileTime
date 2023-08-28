using FileTime.Core.Serialization.Container;

namespace FileTime.Core.Serialization;

[MessagePack.Union(0, typeof(SerializedContainer))]
public interface ISerialized
{
    int Id { get; }
}