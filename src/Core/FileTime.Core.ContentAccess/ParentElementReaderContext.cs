using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

public record ParentElementReaderContext(IContentReader ContentReader, NativePath SubNativePath);