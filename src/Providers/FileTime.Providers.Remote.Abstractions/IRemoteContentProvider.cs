using FileTime.Core.ContentAccess;
using FileTime.Server.Common;

namespace FileTime.Providers.Remote;

public interface IRemoteContentProvider : IContentProvider
{
    Task<IRemoteConnection> GetRemoteConnectionAsync();
    string RemoteProviderName { get; }
    Task InitializeChildren();
}