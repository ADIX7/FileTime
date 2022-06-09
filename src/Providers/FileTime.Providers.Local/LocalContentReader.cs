using FileTime.Core.ContentAccess;

namespace FileTime.Providers.Local;

public class LocalContentReader : IContentReader
{
    private readonly FileStream _readerStream;
    private readonly BinaryReader _binaryReader;
    private bool _disposed;

    public int PreferredBufferSize => 1024 * 1024;
    public long? Position { get; private set; }

    public LocalContentReader(FileStream readerStream)
    {
        _readerStream = readerStream;
        _binaryReader = new BinaryReader(_readerStream);
    }

    public Task<byte[]> ReadBytesAsync(int bufferSize, int? offset = null)
    {
        var max = bufferSize > 0 && bufferSize < PreferredBufferSize ? bufferSize : PreferredBufferSize;

        if (offset != null)
        {
            if (Position == null) Position = 0;
            var buffer = new byte[max];
            var bytesRead = _binaryReader.Read(buffer, offset.Value, max);
            Position += bytesRead;

            if (buffer.Length != bytesRead)
            {
                Array.Resize(ref buffer, bytesRead);
            }
            return Task.FromResult(buffer);
        }
        else
        {
            return Task.FromResult(_binaryReader.ReadBytes(max));
        }
    }

    public void SetPosition(long position)
    {
        Position = position;
    }

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
                _binaryReader.Dispose();
            }
        }
        _disposed = true;
    }
}