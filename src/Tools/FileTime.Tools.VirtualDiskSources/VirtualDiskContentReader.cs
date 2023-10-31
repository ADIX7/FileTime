using FileTime.Core.ContentAccess;

namespace FileTime.Tools.VirtualDiskSources;

public sealed class VirtualDiskContentReader(Stream stream, ICollection<IDisposable> disposables) : IContentReader
{
    public Stream GetStream() => stream;

    public void Dispose()
    {
        stream.Dispose();
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}