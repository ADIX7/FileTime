using FileTime.Core.Models;

namespace FileTime.Avalonia.IconProviders
{
    public interface IIconProvider
    {
        string GetImage(IItem item);
    }
}