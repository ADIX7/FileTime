using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Providers.LocalAdmin;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Local;

public class LocalItemDeleter : IItemDeleter<ILocalContentProvider>
{
    private readonly IAdminContentAccessorFactory _adminContentAccessorFactory;
    private readonly IAdminContentProvider _adminContentProvider;
    private readonly ILogger<LocalItemDeleter> _logger;

    public LocalItemDeleter(
        IAdminContentAccessorFactory adminContentAccessorFactory,
        IAdminContentProvider adminContentProvider,
        ILogger<LocalItemDeleter> logger)
    {
        _adminContentAccessorFactory = adminContentAccessorFactory;
        _adminContentProvider = adminContentProvider;
        _logger = logger;
    }

    public async Task DeleteAsync(ILocalContentProvider contentProvider, FullName fullName)
    {
        _logger.LogTrace("Start deleting item {FullName}", fullName);
        var nativePath = contentProvider.GetNativePath(fullName).Path;

        try
        {
            if (File.Exists(nativePath))
            {
                _logger.LogTrace("File exists with path {Path}", nativePath);
                File.Delete(nativePath);
            }
            else if (Directory.Exists(nativePath))
            {
                _logger.LogTrace("Directory exists with path {Path}", nativePath);
                Directory.Delete(nativePath, true);
            }
            else
            {
                _logger.LogTrace("No file or directory exists with path {Path}", nativePath);
                throw new FileNotFoundException(nativePath);
            }
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Failed to delete item with path {Path}", nativePath);

            if (!_adminContentAccessorFactory.IsAdminModeSupported
                || e is not UnauthorizedAccessException and not IOException)
            {
                _logger.LogTrace(
                    "Admin mode is disabled or exception is not an access denied one, not trying to create {Path} as admin", 
                    nativePath
                );
                throw;
            }

            var adminItemDeleter = await _adminContentAccessorFactory.CreateAdminItemDeleterAsync();
            await adminItemDeleter.DeleteAsync(_adminContentProvider, fullName);
        }
    }
}