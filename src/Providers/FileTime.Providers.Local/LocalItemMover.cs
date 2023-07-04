using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Providers.Local;

public class LocalItemMover : IItemMover<ILocalContentProvider>
{
    public Task RenameAsync(ILocalContentProvider contentProvider, FullName fullName, FullName newPath)
    {
        var source = contentProvider.GetNativePath(fullName);
        var destination = contentProvider.GetNativePath(newPath);
        
        if (File.Exists(source.Path))
        {
            File.Move(source.Path, destination.Path);
        }
        else if (Directory.Exists(source.Path))
        {
            Directory.Move(source.Path, destination.Path);
        }
        else
        {
            throw new FileNotFoundException(source.Path);
        }
        
        return Task.CompletedTask;
    }
}