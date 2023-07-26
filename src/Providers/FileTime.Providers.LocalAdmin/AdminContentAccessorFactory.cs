﻿using System.Diagnostics;
using FileTime.Providers.Remote;
using FileTime.Server.Common;
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
        => await CreateHelperAsync<IRemoteItemCreator>();

    public async Task<IRemoteItemDeleter> CreateAdminItemDeleterAsync()
        => await CreateHelperAsync<IRemoteItemDeleter>();

    public async Task<IRemoteItemMover> CreateAdminItemMoverAsync() 
        => await CreateHelperAsync<IRemoteItemMover>();

    private async Task<T> CreateHelperAsync<T>()
        where T : class, IInitable<IRemoteConnection, string>
    {
        await _adminElevationManager.CreateAdminInstanceIfNecessaryAsync();
        var connection = await _adminElevationManager.CreateConnectionAsync();
        
        Debug.Assert(connection != null);
        
        var helper = _serviceProvider.GetInitableResolver(
                connection, 
                _adminElevationManager.ProviderName)
            .GetRequiredService<T>();
        return helper;
    }
}