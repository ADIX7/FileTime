using FileTime.Core.ContentAccess;
using FileTime.Server.Common;
using InitableService;

namespace FileTime.Providers.Remote;

public interface IRemoteItemDeleter : IItemDeleter<IRemoteContentProvider>, IInitable<IRemoteConnection, string>
{
    
}