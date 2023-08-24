using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.Server.Common;

namespace FileTime.Providers.Remote;

public sealed class RemoteContentProvider : ContentProviderBase, IRemoteContentProvider
{
    public string RemoteProviderName { get; }
    private readonly Func<Task<IRemoteConnection>> _remoteConnectionProvider;

    public RemoteContentProvider(
        ITimelessContentProvider timelessContentProvider,
        Func<Task<IRemoteConnection>> remoteConnectionProvider,
        string remoteName,
        string name)
        : base(name, timelessContentProvider)
    {
        RemoteProviderName = remoteName;
        _remoteConnectionProvider = remoteConnectionProvider;
    }

    public async Task<IRemoteConnection> GetRemoteConnectionAsync()
        => await _remoteConnectionProvider();

    //TODO implement
    public override Task<IItem> GetItemByNativePathAsync(
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default) =>
        throw new NotImplementedException();

    //TODO: make it async
    public override async ValueTask<NativePath> GetNativePathAsync(FullName fullName)
    {
        var remoteFullname = new FullName(ConvertLocalFullNameToRemote(fullName));

        var connection = await GetRemoteConnectionAsync();
        var remoteNativePath = await connection.GetNativePathAsync(remoteFullname);
        return new NativePath(remoteNativePath!.Path);
    }

    public override FullName GetFullName(NativePath nativePath) => throw new NotImplementedException();

    public override Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public override Task<bool> CanHandlePathAsync(NativePath path) => throw new NotImplementedException();
    public override VolumeSizeInfo? GetVolumeSizeInfo(FullName path) => throw new NotImplementedException();

    private string ConvertLocalFullNameToRemote(FullName fullName)
    {
        var remotePath =
            RemoteProviderName
            + Constants.SeparatorChar
            + fullName.Path[Name.Length..];
        return remotePath;
    }

    private FullName ConvertRemoteFullnameToLocal(string remotePath)
    {
        var localPath =
            Name
            + Constants.SeparatorChar
            + remotePath[RemoteProviderName.Length..];
        return new FullName(localPath);
    }
}