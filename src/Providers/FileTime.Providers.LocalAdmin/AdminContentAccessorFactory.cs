using System.Diagnostics;
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

    public async Task<IRemoteItemCreator> CreateAdminItemCreatorAsync()
    {
        await _adminElevationManager.CreateAdminInstanceIfNecessaryAsync();
        var connection = await _adminElevationManager.CreateConnectionAsync();
        
        Debug.Assert(connection != null);
        
        var adminItemCreator = _serviceProvider.GetInitableResolver(
                connection, 
                _adminElevationManager.ProviderName)
            .GetRequiredService<IRemoteItemCreator>();
        return adminItemCreator;
    }

    public async Task<IRemoteItemDeleter> CreateAdminItemDeleterAsync()
    {
        await _adminElevationManager.CreateAdminInstanceIfNecessaryAsync();
        var connection = await _adminElevationManager.CreateConnectionAsync();
        
        Debug.Assert(connection != null);
        
        var adminItemDeleter = _serviceProvider.GetInitableResolver(
                connection, 
                _adminElevationManager.ProviderName)
            .GetRequiredService<IRemoteItemDeleter>();
        return adminItemDeleter;
    }
}