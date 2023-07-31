using FileTime.App.Core.Models;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services;

public interface IItemNameConverterService
{
    List<ItemNamePart> GetDisplayName(string name, string? searchText);
    string GetFileExtension(string fullName);
    string GetFileName(string fullName);
}