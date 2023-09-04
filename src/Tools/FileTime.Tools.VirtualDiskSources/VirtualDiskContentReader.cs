using FileTime.Core.ContentAccess;

namespace FileTime.Tools.VirtualDiskSources;

public class VirtualDiskContentReader : IContentReader
{
    private readonly Stream _stream;
    private readonly ICollection<IDisposable> _disposables;
    public int PreferredBufferSize => 1024 * 1024;
    public long? Position => _stream.Position;

    public VirtualDiskContentReader(Stream stream, ICollection<IDisposable> disposables)
    {
        _stream = stream;
        _disposables = disposables;
    }

    public async Task<byte[]> ReadBytesAsync(int bufferSize, int? offset = null)
    {
        var data = new byte[bufferSize];
        var read = await _stream.ReadAsync(data, offset ?? 0, bufferSize);

        return data[..read].ToArray();
    }

    public void SetPosition(long position) => _stream.Seek(position, SeekOrigin.Begin);

    public Stream AsStream() => _stream;


    public void Dispose()
    {
        _stream.Dispose();
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}