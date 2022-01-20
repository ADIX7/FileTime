using FileTime.Core.Models;

public interface IIconProvider
{
    string GetImage(IItem item);
}