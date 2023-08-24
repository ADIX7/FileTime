using FileTime.Core.Models;
using FileTime.Providers.Remote;

namespace FileTime.Providers.LocalAdmin;

public interface IAdminContentAccessorFactory
{
    bool IsAdminModeSupported { get; }
    Task<RemoteItemCreator> CreateAdminItemCreatorAsync();
    Task<RemoteItemDeleter> CreateAdminItemDeleterAsync();
    Task<RemoteItemMover> CreateAdminItemMoverAsync();
    Task<RemoteContentWriter> CreateContentWriterAsync(NativePath nativePath);
}