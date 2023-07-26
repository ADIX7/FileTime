using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Server.Common;
using InitableService;

namespace FileTime.Providers.Remote;

public interface IRemoteContentWriter :
    IContentWriter,
    IInitable<IRemoteConnection, string, NativePath, Guid>
{
}