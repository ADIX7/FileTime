using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Providers.Local;

public class LocalItemDeleter : IItemDeleter<ILocalContentProvider>
{
    public Task DeleteAsync(ILocalContentProvider contentProvider, FullName fullName)
    {
        var nativePath = contentProvider.GetNativePath(fullName).Path;

        if (File.Exists(nativePath))
        {
            File.Delete(nativePath);
        }
        else if (Directory.Exists(nativePath))
        {
            Directory.Delete(nativePath, true);
        }
        else
        {
            throw new FileNotFoundException(nativePath);
        }

        return Task.CompletedTask;
    }
}