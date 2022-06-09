namespace FileTime.Core.ContentAccess;

public class ContentAccessStream : Stream
{
    private readonly IContentReader? _contentReader;
    private readonly IContentWriter? _contentWriter;
    public override bool CanRead => _contentReader != null;

    public override bool CanSeek => _contentReader != null;

    public override bool CanWrite => _contentWriter != null;

    public override long Length => throw new NotImplementedException();

    public override long Position
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public ContentAccessStream(IContentReader contentReader)
    {
        _contentReader = contentReader;
    }

    public ContentAccessStream(IContentWriter contentWriter)
    {
        _contentWriter = contentWriter;
    }

    public override void Flush()
    {
        if (_contentWriter == null) throw new NotSupportedException();
        Task.Run(async () => await _contentWriter.FlushAsync()).Wait();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_contentReader == null) throw new IOException("This stream is not readable");
        var dataTask = Task.Run(async () => await _contentReader.ReadBytesAsync(count, offset));
        dataTask.Wait();
        var data = dataTask.Result;

        if (data.Length > count) throw new Exception("More bytes has been read than requested");
        Array.Copy(data, buffer, data.Length);
        return data.Length;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        if (_contentReader == null) throw new NotSupportedException();

        var newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _contentReader.Position ?? 0 + offset,
            _ => throw new NotSupportedException()
        };
        _contentReader.SetPosition(newPosition);
        return newPosition;
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_contentWriter == null) throw new NotSupportedException();
        var data = buffer;
        if (buffer.Length != count)
        {
            data = new byte[count];
            Array.Copy(buffer, data, count);
        }

        Task.Run(async () => await _contentWriter.WriteBytesAsync(data, offset)).Wait();
    }
}