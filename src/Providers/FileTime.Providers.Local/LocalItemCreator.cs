using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Providers.LocalAdmin;

namespace FileTime.Providers.Local;

public class LocalItemCreator : ItemCreatorBase<ILocalContentProvider>
{
    private readonly IAdminContentAccessorFactory _adminContentAccessorFactory;
    private readonly IAdminContentProvider _adminContentProvider;

    public LocalItemCreator(
        IAdminContentAccessorFactory adminContentAccessorFactory,
        IAdminContentProvider adminContentProvider)
    {
        _adminContentAccessorFactory = adminContentAccessorFactory;
        _adminContentProvider = adminContentProvider;
    }

    public override async Task CreateContainerAsync(ILocalContentProvider contentProvider, FullName fullName)
    {
        var path = contentProvider.GetNativePath(fullName).Path;
        if (Directory.Exists(path)) return;

        try
        {
            Directory.CreateDirectory(path);
        }
        catch (UnauthorizedAccessException)
        {
            if (!_adminContentAccessorFactory.IsAdminModeSupported) throw;

            var adminContentAccessor = await _adminContentAccessorFactory.CreateAdminItemCreatorAsync();
            await adminContentAccessor.CreateContainerAsync(_adminContentProvider, fullName);
        }
    }

    public override async Task CreateElementAsync(ILocalContentProvider contentProvider, FullName fullName)
    {
        var path = contentProvider.GetNativePath(fullName).Path;
        if (File.Exists(path)) return;
        
        try
        {
            await using (File.Create(path))
            {
            }
        }
        catch (UnauthorizedAccessException)
        {
            if (!_adminContentAccessorFactory.IsAdminModeSupported) throw;

            var adminContentAccessor = await _adminContentAccessorFactory.CreateAdminItemCreatorAsync();
            await adminContentAccessor.CreateElementAsync(_adminContentProvider, fullName);
        }
    }
}