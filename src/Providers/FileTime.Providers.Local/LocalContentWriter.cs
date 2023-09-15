using FileTime.Core.ContentAccess;

namespace FileTime.Providers.Local;

public class LocalContentWriter : IContentWriter
{
    private readonly FileStream _writerStream;
    private bool _disposed;

    public LocalContentWriter(FileStream writerStream)
    {
        _writerStream = writerStream;
    }

    public Stream GetStream() => _writerStream;

    ~LocalContentWriter()
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
                _writerStream.Dispose();
            }
        }
        _disposed = true;
    }
}