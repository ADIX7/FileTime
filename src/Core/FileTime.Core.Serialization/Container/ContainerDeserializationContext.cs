using FileTime.Core.ContentAccess;

namespace FileTime.Core.Serialization.Container;

public record ContainerDeserializationContext(IContentProvider ContentProvider);