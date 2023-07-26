using FileTime.Providers.Remote;

namespace FileTime.Providers.LocalAdmin;

public interface IAdminContentAccessorFactory
{
    bool IsAdminModeSupported { get; }
    Task<IRemoteItemCreator> CreateAdminItemCreatorAsync();
    Task<IRemoteItemDeleter> CreateAdminItemDeleterAsync();
}