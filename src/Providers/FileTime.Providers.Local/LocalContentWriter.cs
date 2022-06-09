using FileTime.Core.ContentAccess;

namespace FileTime.Providers.Local;

public class LocalContentWriter : IContentWriter
{
    private readonly FileStream _writerStream;
    private readonly BinaryWriter _binaryWriter;
    private bool disposed;
    public int PreferredBufferSize => 1024 * 1024;

    public LocalContentWriter(FileStream writerStream)
    {
        _writerStream = writerStream;
        _binaryWriter = new BinaryWriter(_writerStream);
    }

    public Task WriteBytesAsync(byte[] data, int? index = null)
    {
        if (index != null)
        {
            _binaryWriter.Write(data, index.Value, data.Length);
        }
        else
        {
            _binaryWriter.Write(data);
        }
        return Task.CompletedTask;
    }

    public Task FlushAsync()
    {
        _binaryWriter.Flush();
        return Task.CompletedTask;
    }

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
        if (!disposed)
        {
            if (disposing)
            {
                _writerStream.Dispose();
                _binaryWriter.Dispose();
            }
        }
        disposed = true;
    }
}