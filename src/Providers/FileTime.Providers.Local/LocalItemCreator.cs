using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Providers.Local;

public class LocalItemCreator : ItemCreatorBase<ILocalContentProvider>
{
    public override Task CreateContainerAsync(ILocalContentProvider contentProvider, FullName fullName)
    {
        var path = contentProvider.GetNativePath(fullName).Path;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        return Task.CompletedTask;
    }

    public override async Task CreateElementAsync(ILocalContentProvider contentProvider, FullName fullName)
    {
        var path = contentProvider.GetNativePath(fullName).Path;
        await using (File.Create(path))
        {
        }
    }
}