using FileTime.Core.ContentAccess;
using SharpCompress.Archives;

namespace FileTime.Tools.Compression.ContentProvider;

public sealed class CompressedContentReader : IContentReader
{
    private readonly IDisposable[] _disposables;
    private readonly Stream _stream;

    public int PreferredBufferSize => 1024 * 1024;
    public long? Position => _stream.Position;

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

    public async Task<byte[]> ReadBytesAsync(int bufferSize, int? offset = null)
    {
        var data = new byte[bufferSize];
        var read = await _stream.ReadAsync(data, offset ?? 0, bufferSize);

        return data[..read].ToArray();
    }

    public void SetPosition(long position) => _stream.Seek(position, SeekOrigin.Begin);

    public Stream AsStream() => _stream;
}