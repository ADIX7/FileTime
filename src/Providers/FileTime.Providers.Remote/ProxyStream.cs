namespace FileTime.Providers.Remote;

internal sealed class ProxyStream : Stream
{
    private readonly RemoteContentWriter _remoteContentWriter;

    public ProxyStream(RemoteContentWriter remoteContentWriter)
    {
        _remoteContentWriter = remoteContentWriter;
    }

    public override void Flush() => InitializeRemoteWriterAndRun(() => _remoteContentWriter.FlushAsync());

    public override int Read(byte[] buffer, int offset, int count) 
        => InitializeRemoteWriterAndRun(() => _remoteContentWriter.ReadAsync(buffer, offset, count));

    public override long Seek(long offset, SeekOrigin origin) 
        => InitializeRemoteWriterAndRun(() => _remoteContentWriter.SeekAsync(offset, origin));

    public override void SetLength(long value) => InitializeRemoteWriterAndRun(() => _remoteContentWriter.SetLengthAsync(value));

    public override void Write(byte[] buffer, int offset, int count) => InitializeRemoteWriterAndRun(() => _remoteContentWriter.WriteAsync(buffer, offset, count));

    public override bool CanRead => InitializeRemoteWriterAndRun(() => _remoteContentWriter.CanReadAsync());

    public override bool CanSeek => InitializeRemoteWriterAndRun(() => _remoteContentWriter.CanSeekAsync());

    public override bool CanWrite => InitializeRemoteWriterAndRun(() => _remoteContentWriter.CanWriteAsync());

    public override long Length => InitializeRemoteWriterAndRun(() => _remoteContentWriter.GetLengthAsync());

    public override long Position
    {
        get => InitializeRemoteWriterAndRun(() => _remoteContentWriter.GetPositionAsync());
        set => InitializeRemoteWriterAndRun(() => _remoteContentWriter.SetPositionAsync(value));
    }

    private void InitializeRemoteWriterAndRun(Func<Task> func)
    {
        var task = Task.Run(async () =>
        {
            await _remoteContentWriter.InitializeRemoteWriterAsync();
            await func();
        });
        task.Wait();
    }

    private T InitializeRemoteWriterAndRun<T>(Func<Task<T>> func)
    {
        var task = Task.Run(async () =>
        {
            await _remoteContentWriter.InitializeRemoteWriterAsync();
            return await func();
        });
        task.Wait();
        return task.Result;
    }
}