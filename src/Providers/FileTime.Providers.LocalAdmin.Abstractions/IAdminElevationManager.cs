using FileTime.Server.Common;

namespace FileTime.Providers.LocalAdmin;

public interface IAdminElevationManager
{
    bool IsAdminModeSupported { get; }
    bool IsAdminInstanceRunning { get; }
    Task<IRemoteConnection> CreateConnectionAsync();
    string ProviderName { get; }
    Task CreateAdminInstanceIfNecessaryAsync(string? confirmationMessage = null);
}