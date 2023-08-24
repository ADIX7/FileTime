using FileTime.Providers.Remote;

namespace FileTime.Providers.LocalAdmin;

public interface IAdminElevationManager
{
    bool IsAdminModeSupported { get; }
    bool IsAdminInstanceRunning { get; }
    Task<IRemoteContentProvider> GetRemoteContentProviderAsync();
    string ProviderName { get; }
    Task CreateAdminInstanceIfNecessaryAsync(string? confirmationMessage = null);
}