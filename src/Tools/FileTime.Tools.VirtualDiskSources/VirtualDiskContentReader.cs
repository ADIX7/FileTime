using FileTime.Core.ContentAccess;

namespace FileTime.Tools.VirtualDiskSources;

public sealed class VirtualDiskContentReader : IContentReader
{
    private readonly Stream _stream;
    private readonly ICollection<IDisposable> _disposables;

    public VirtualDiskContentReader(Stream stream, ICollection<IDisposable> disposables)
    {
        _stream = stream;
        _disposables = disposables;
    }

    public Stream GetStream() => _stream;

    public void Dispose()
    {
        _stream.Dispose();
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}