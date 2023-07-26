using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Providers.LocalAdmin;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Local;

public class LocalItemMover : IItemMover<ILocalContentProvider>
{
    private readonly IAdminContentAccessorFactory _adminContentAccessorFactory;
    private readonly IAdminContentProvider _adminContentProvider;
    private readonly ILogger<LocalItemMover> _logger;

    public LocalItemMover(
        IAdminContentAccessorFactory adminContentAccessorFactory,
        IAdminContentProvider adminContentProvider,
        ILogger<LocalItemMover> logger)
    {
        _adminContentAccessorFactory = adminContentAccessorFactory;
        _adminContentProvider = adminContentProvider;
        _logger = logger;
    }
    
    public async Task RenameAsync(ILocalContentProvider contentProvider, FullName fullName, FullName newPath)
    {
        _logger.LogTrace("Start renaming item {FullName}", fullName);
        try
        {
            var source = contentProvider.GetNativePath(fullName);
            var destination = contentProvider.GetNativePath(newPath);

            if (File.Exists(source.Path))
            {
                _logger.LogTrace("File exists with path {Path}", fullName);
                File.Move(source.Path, destination.Path);
            }
            else if (Directory.Exists(source.Path))
            {
                _logger.LogTrace("Directory exists with path {Path}", fullName);
                Directory.Move(source.Path, destination.Path);
            }
            else
            {
                _logger.LogTrace("No file or directory exists with path {Path}", fullName);
                throw new FileNotFoundException(source.Path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to rename item {From} to {To}", fullName, newPath);

            if (!_adminContentAccessorFactory.IsAdminModeSupported
                || ex is not UnauthorizedAccessException and not IOException)
            {
                _logger.LogTrace(
                    "Admin mode is disabled or exception is not an access denied one, not trying to rename {Path} as admin", 
                    fullName
                );
                throw;
            }

            var adminItemMover = await _adminContentAccessorFactory.CreateAdminItemMoverAsync();
            await adminItemMover.RenameAsync(_adminContentProvider, fullName, newPath);
        }
    }
}