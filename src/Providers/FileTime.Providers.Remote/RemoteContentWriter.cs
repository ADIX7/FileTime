using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.Providers.Remote;

public class RemoteContentWriter : IContentWriter, IInitable<IRemoteContentProvider, string, NativePath, Guid>
{
    private IRemoteContentProvider _remoteContentProvider = null!;
    private string _remoteContentProviderId = null!;
    private NativePath _nativePath = null!;
    private string _transactionId = null!;
    private bool _isRemoteWriterInitialized;

    public void Init(
        IRemoteContentProvider remoteContentProvider,
        string remoteContentProviderId,
        NativePath nativePath,
        Guid transactionId)
    {
        _remoteContentProvider = remoteContentProvider;
        _remoteContentProviderId = remoteContentProviderId;
        _nativePath = nativePath;
        _transactionId = transactionId.ToString();
    }

    public void Dispose()
    {
        if (!_isRemoteWriterInitialized) return;
        Task.Run(async () => await (await _remoteContentProvider.GetRemoteConnectionAsync()).CloseWriterAsync(_transactionId));
    }

    public Stream GetStream() => new ProxyStream(this);

    public async Task InitializeRemoteWriterAsync()
    {
        if (_isRemoteWriterInitialized) return;
        _isRemoteWriterInitialized = true;
        await (await _remoteContentProvider.GetRemoteConnectionAsync()).InitializeRemoteWriter(_remoteContentProviderId, _transactionId, _nativePath);
    }

    public async Task FlushAsync()
    {
        await InitializeRemoteWriterAsync();
        await (await _remoteContentProvider.GetRemoteConnectionAsync()).FlushAsync(_transactionId);
    }

    public async Task<int> ReadAsync(byte[] buffer, int offset, int count)
    {
        await InitializeRemoteWriterAsync();
        return await (await _remoteContentProvider.GetRemoteConnectionAsync()).ReadAsync(_transactionId, buffer, offset, count);
    }

    public async Task<long> SeekAsync(long offset, SeekOrigin origin)
    {
        await InitializeRemoteWriterAsync();
        return await (await _remoteContentProvider.GetRemoteConnectionAsync()).SeekAsync(_transactionId, offset, origin);
    }

    public async Task SetLengthAsync(long value)
    {
        await InitializeRemoteWriterAsync();
        await (await _remoteContentProvider.GetRemoteConnectionAsync()).SetLengthAsync(_transactionId, value);
    }

    public async Task WriteAsync(byte[] buffer, int offset, int count)
    {
        await InitializeRemoteWriterAsync();
        await (await _remoteContentProvider.GetRemoteConnectionAsync()).WriteAsync(_transactionId, buffer, offset, count);
    }

    public async Task<bool> CanReadAsync()
    {
        await InitializeRemoteWriterAsync();
        return await (await _remoteContentProvider.GetRemoteConnectionAsync()).CanReadAsync(_transactionId);
    }

    public async Task<bool> CanSeekAsync()
    {
        await InitializeRemoteWriterAsync();
        return await (await _remoteContentProvider.GetRemoteConnectionAsync()).CanSeekAsync(_transactionId);
    }

    public async Task<bool> CanWriteAsync()
    {
        await InitializeRemoteWriterAsync();
        return await (await _remoteContentProvider.GetRemoteConnectionAsync()).CanWriteAsync(_transactionId);
    }

    public async Task<long> GetLengthAsync()
    {
        await InitializeRemoteWriterAsync();
        return await (await _remoteContentProvider.GetRemoteConnectionAsync()).GetLengthAsync(_transactionId);
    }

    public async Task<long> GetPositionAsync()
    {
        await InitializeRemoteWriterAsync();
        return await (await _remoteContentProvider.GetRemoteConnectionAsync()).GetPositionAsync(_transactionId);
    }

    public async Task SetPositionAsync(long position)
    {
        await InitializeRemoteWriterAsync();
        await (await _remoteContentProvider.GetRemoteConnectionAsync()).SetPositionAsync(_transactionId, position);
    }
}