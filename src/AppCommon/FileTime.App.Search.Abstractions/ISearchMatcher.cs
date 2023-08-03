using FileTime.Core.Models;

namespace FileTime.App.Search;

public interface ISearchMatcher
{
    Task<bool> IsItemMatchAsync(IItem item);
    List<ItemNamePart> GetDisplayName(IItem item);
}