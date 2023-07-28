using FileTime.Core.Models;
using FileTime.Server.Common;

namespace FileTime.Providers.Remote;

public class RemoteContentWriter : IRemoteContentWriter
{
    private IRemoteConnection _remoteConnection = null!;
    private string _remoteContentProviderId = null!;
    private NativePath _nativePath = null!;
    private string _transactionId = null!;
    private bool _isRemoteWriterInitialized;

    public void Init(
        IRemoteConnection remoteConnection,
        string remoteContentProviderId,
        NativePath nativePath,
        Guid transactionId)
    {
        _remoteConnection = remoteConnection;
        _remoteContentProviderId = remoteContentProviderId;
        _nativePath = nativePath;
        _transactionId = transactionId.ToString();
    }

    public void Dispose()
    {
        if (!_isRemoteWriterInitialized) return;
        _remoteConnection.CloseWriterAsync(_transactionId);
    }

    public int PreferredBufferSize => 10 * 1024 * 1024;

    public async Task WriteBytesAsync(byte[] data, int? index = null, CancellationToken cancellationToken = default)
    {
        if (!_isRemoteWriterInitialized) await InitializeRemoteWriter(_nativePath);
        await _remoteConnection.WriteBytesAsync(_transactionId, data, index, cancellationToken);
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRemoteWriterInitialized) return;
        await _remoteConnection.FlushWriterAsync(_transactionId, cancellationToken);
    }

    private async Task InitializeRemoteWriter(NativePath nativePath)
    {
        _isRemoteWriterInitialized = true;
        await _remoteConnection.InitializeRemoteWriter(_remoteContentProviderId, _transactionId, nativePath);
    }
}