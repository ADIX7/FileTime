using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Providers.LocalAdmin;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Local;

public class LocalItemCreator : ItemCreatorBase<ILocalContentProvider>
{
    private readonly IAdminContentAccessorFactory _adminContentAccessorFactory;
    private readonly IAdminElevationManager _adminElevationManager;
    private readonly ILogger<LocalItemCreator> _logger;

    public LocalItemCreator(
        IAdminContentAccessorFactory adminContentAccessorFactory,
        IAdminElevationManager adminElevationManager,
        ILogger<LocalItemCreator> logger)
    {
        _adminContentAccessorFactory = adminContentAccessorFactory;
        _adminElevationManager = adminElevationManager;
        _logger = logger;
    }

    public override async Task CreateContainerAsync(ILocalContentProvider contentProvider, FullName fullName)
    {
        _logger.LogTrace("Start creating container {FullName}", fullName);
        var path = contentProvider.GetNativePath(fullName).Path;
        if (Directory.Exists(path))
        {
            _logger.LogTrace("Container with path {Path} already exists", path);
            return;
        }

        try
        {
            _logger.LogTrace("Trying to create container with path {Path}", path);
            Directory.CreateDirectory(path);
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogDebug(e, "Failed to create container with path {Path}", path);
            if (!_adminContentAccessorFactory.IsAdminModeSupported)
            {
                _logger.LogTrace("Admin mode is disabled, not trying to create {Path} as admin", path);
                throw;
            }

            var adminItemCreator = await _adminContentAccessorFactory.CreateAdminItemCreatorAsync();
            var remoteContentProvider = await _adminElevationManager.GetRemoteContentProviderAsync();
            await adminItemCreator.CreateContainerAsync(remoteContentProvider, fullName);
        }
    }

    public override async Task CreateElementAsync(ILocalContentProvider contentProvider, FullName fullName)
    {
        _logger.LogTrace("Start creating element {FullName}", fullName);
        var path = contentProvider.GetNativePath(fullName).Path;
        if (File.Exists(path))
        {
            _logger.LogTrace("Element with path {Path} already exists", path);
            return;
        }

        try
        {
            _logger.LogTrace("Trying to create element with path {Path}", path);
            var directory = Path.GetDirectoryName(path);
            if (directory is { } && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
            
            await using (File.Create(path))
            {
            }
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogDebug(e, "Failed to create element with path {Path}", path);
            if (!_adminContentAccessorFactory.IsAdminModeSupported)
            {
                _logger.LogTrace("Admin mode is disabled, not trying to create {Path} as admin", path);
                throw;
            }

            var adminItemCreator = await _adminContentAccessorFactory.CreateAdminItemCreatorAsync();
            var remoteContentProvider = await _adminElevationManager.GetRemoteContentProviderAsync();
            await adminItemCreator.CreateElementAsync(remoteContentProvider, fullName);
        }
    }
}