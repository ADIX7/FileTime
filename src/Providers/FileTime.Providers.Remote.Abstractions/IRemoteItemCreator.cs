using FileTime.Core.ContentAccess;
using FileTime.Server.Common;
using InitableService;

namespace FileTime.Providers.Remote;

public interface IRemoteItemCreator : 
    IItemCreator<IRemoteContentProvider>,
    IInitable<IRemoteConnection, string>
{
    
}