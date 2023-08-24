using System.Diagnostics;
using FileTime.Core.Models;
using FileTime.Providers.Remote;
using InitableService;

namespace FileTime.Providers.LocalAdmin;

public class AdminContentAccessorFactory : IAdminContentAccessorFactory
{
    private readonly IAdminElevationManager _adminElevationManager;
    private readonly IServiceProvider _serviceProvider;

    public AdminContentAccessorFactory(
        IAdminElevationManager adminElevationManager,
        IServiceProvider serviceProvider
    )
    {
        _adminElevationManager = adminElevationManager;
        _serviceProvider = serviceProvider;
    }

    public bool IsAdminModeSupported => _adminElevationManager.IsAdminModeSupported;

    public async Task<RemoteItemCreator> CreateAdminItemCreatorAsync()
        => await CreateHelperAsync<RemoteItemCreator>();

    public async Task<RemoteItemDeleter> CreateAdminItemDeleterAsync()
        => await CreateHelperAsync<RemoteItemDeleter>();

    public async Task<RemoteItemMover> CreateAdminItemMoverAsync()
        => await CreateHelperAsync<RemoteItemMover>();

    public async Task<RemoteContentWriter> CreateContentWriterAsync(NativePath nativePath)
    {
        await _adminElevationManager.CreateAdminInstanceIfNecessaryAsync();
        var connection = await _adminElevationManager.GetRemoteContentProviderAsync();
        var contentWriter = _serviceProvider.GetInitableResolver(
            connection,
            _adminElevationManager.ProviderName,
            nativePath,
            Guid.NewGuid()
        ).GetRequiredService<RemoteContentWriter>();

        return contentWriter;
    }

    private async Task<T> CreateHelperAsync<T>()
        where T : class, IInitable<IRemoteContentProvider>
    {
        await _adminElevationManager.CreateAdminInstanceIfNecessaryAsync();
        var connection = await _adminElevationManager.GetRemoteContentProviderAsync();

        Debug.Assert(connection != null);

        var helper = _serviceProvider.GetInitableResolver(
                connection)
            .GetRequiredService<T>();
        return helper;
    }
}