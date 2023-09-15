using FileTime.Core.ContentAccess;

namespace FileTime.Providers.Local;

public class LocalContentReader : IContentReader
{
    private readonly FileStream _readerStream;
    private bool _disposed;

    public LocalContentReader(FileStream readerStream)
    {
        _readerStream = readerStream;
    }

    public Stream GetStream() => _readerStream;

    ~LocalContentReader()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _readerStream.Dispose();
            }
        }
        _disposed = true;
    }
}