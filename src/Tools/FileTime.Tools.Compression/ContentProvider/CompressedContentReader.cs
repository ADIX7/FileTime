using FileTime.Core.ContentAccess;
using SharpCompress.Archives;

namespace FileTime.Tools.Compression.ContentProvider;

public sealed class CompressedContentReader : IContentReader
{
    private readonly IDisposable[] _disposables;
    private readonly Stream _stream;

    public CompressedContentReader(IArchiveEntry entry, IDisposable[] disposables)
    {
        _disposables = disposables;
        _stream = entry.OpenEntryStream();
    }

    public void Dispose()
    {
        _stream.Dispose();
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    public Stream GetStream() => _stream;
}