using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Serialization.Container;
using FileTime.Core.Timeline;
using FileTime.Server.Common;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Providers.Remote;

public sealed class RemoteContentProvider : ContentProviderBase, IRemoteContentProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<Task<IRemoteConnection>> _remoteConnectionProvider;
    private readonly SemaphoreSlim _initializeSemaphore = new(1, 1);
    private bool _initialized;

    public string RemoteProviderName { get; }

    public RemoteContentProvider(
        ITimelessContentProvider timelessContentProvider,
        IServiceProvider serviceProvider,
        Func<Task<IRemoteConnection>> remoteConnectionProvider,
        string remoteName,
        string name)
        : base(name, timelessContentProvider)
    {
        RemoteProviderName = remoteName;
        _serviceProvider = serviceProvider;
        _remoteConnectionProvider = remoteConnectionProvider;
    }

    public async Task<IRemoteConnection> GetRemoteConnectionAsync()
        => await _remoteConnectionProvider();

    public async Task InitializeChildren()
    {
        await _initializeSemaphore.WaitAsync();
        try
        {
            if (_initialized) return;
            
            //TODO: loading indicator
            
            var connection = await GetRemoteConnectionAsync();
            var children = await connection.GetChildren(RemoteProviderName, RemoteProviderName);
            _initialized = true;
        }
        finally
        {
            _initializeSemaphore.Release();
        }
    }

    //TODO implement
    public override async Task<IItem> GetItemByNativePathAsync(
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
    {
        if (nativePath.Path == string.Empty)
        {
            return this;
        }

        var connection = await GetRemoteConnectionAsync();
        var serialized = await connection.GetItemByNativePathAsync(
            RemoteProviderName,
            nativePath,
            pointInTime,
            forceResolve,
            forceResolvePathType,
            itemInitializationSettings
        );

        if (serialized is SerializedContainer serializedContainer)
        {
            var containerDeserializer = _serviceProvider.GetRequiredService<ContainerDeserializer>();
            var container = containerDeserializer.Deserialize(
                serializedContainer,
                new ContainerDeserializationContext(this)
            );

            return container.Container;
        }

        throw new NotSupportedException();
    }

    public override async ValueTask<NativePath> GetNativePathAsync(FullName fullName)
    {
        var remoteFullname = new FullName(ConvertLocalFullNameToRemote(fullName));

        var connection = await GetRemoteConnectionAsync();
        var remoteNativePath = await connection.GetNativePathAsync(RemoteProviderName, remoteFullname);
        return new NativePath(remoteNativePath.Path);
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