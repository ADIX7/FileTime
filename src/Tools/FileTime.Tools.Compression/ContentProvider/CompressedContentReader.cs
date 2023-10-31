using FileTime.Core.ContentAccess;
using SharpCompress.Archives;

namespace FileTime.Tools.Compression.ContentProvider;

public sealed class CompressedContentReader(IArchiveEntry entry, IDisposable[] disposables) : IContentReader
{
    private readonly Stream _stream = entry.OpenEntryStream();

    public void Dispose()
    {
        _stream.Dispose();
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }

    public Stream GetStream() => _stream;
}