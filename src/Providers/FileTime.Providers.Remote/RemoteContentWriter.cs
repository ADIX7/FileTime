using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Server.Common;
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

    public int PreferredBufferSize => 10 * 1024 * 1024;

    public async Task WriteBytesAsync(byte[] data, int? index = null, CancellationToken cancellationToken = default)
    {
        if (!_isRemoteWriterInitialized) await InitializeRemoteWriter(_nativePath);
        await (await _remoteContentProvider.GetRemoteConnectionAsync()).WriteBytesAsync(_transactionId, data, index, cancellationToken);
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRemoteWriterInitialized) return;
        await (await _remoteContentProvider.GetRemoteConnectionAsync()).FlushWriterAsync(_transactionId, cancellationToken);
    }

    public Stream AsStream() => new ContentAccessStream(this);

    private async Task InitializeRemoteWriter(NativePath nativePath)
    {
        _isRemoteWriterInitialized = true;
        await (await _remoteContentProvider.GetRemoteConnectionAsync()).InitializeRemoteWriter(_remoteContentProviderId, _transactionId, nativePath);
    }
}