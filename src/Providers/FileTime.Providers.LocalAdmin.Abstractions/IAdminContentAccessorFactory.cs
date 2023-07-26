using FileTime.Core.Models;
using FileTime.Providers.Remote;

namespace FileTime.Providers.LocalAdmin;

public interface IAdminContentAccessorFactory
{
    bool IsAdminModeSupported { get; }
    Task<IRemoteItemCreator> CreateAdminItemCreatorAsync();
    Task<IRemoteItemDeleter> CreateAdminItemDeleterAsync();
    Task<IRemoteItemMover> CreateAdminItemMoverAsync();
    Task<IRemoteContentWriter> CreateContentWriterAsync(NativePath nativePath);
}