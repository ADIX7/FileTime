using FileTime.Providers.LocalAdmin;
using FileTime.Providers.Remote;
using FileTime.Server.Common;

namespace FileTime.Server.App;

public class DummyAdminElevationManager : IAdminElevationManager
{
    public bool IsAdminInstanceRunning => throw new NotImplementedException();
    public Task<IRemoteContentProvider> GetRemoteContentProviderAsync() => throw new NotImplementedException();

    public string ProviderName => throw new NotImplementedException();
    public Task CreateAdminInstanceIfNecessaryAsync(string? confirmationMessage = null) => throw new NotImplementedException();
    public bool IsAdminModeSupported => false;
}