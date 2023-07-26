using FileTime.Core.ContentAccess;
using FileTime.Server.Common;
using InitableService;

namespace FileTime.Providers.Remote;

public interface IRemoteItemMover : IItemMover<IRemoteContentProvider>, IInitable<IRemoteConnection, string>
{

}