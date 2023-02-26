using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.Core.Models;

namespace FileTime.App.Search;

public class NameContainsMatcher : ISearchMatcher
{
    private readonly IItemNameConverterService _itemNameConverterService;
    private readonly string _searchText;

    public NameContainsMatcher(IItemNameConverterService itemNameConverterService, string searchText)
    {
        _itemNameConverterService = itemNameConverterService;
        _searchText = searchText;
    }

    public Task<bool> IsItemMatchAsync(IItem item) => Task.FromResult(item.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

    public List<ItemNamePart> GetDisplayName(IItem item) => _itemNameConverterService.GetDisplayName(item.DisplayName, _searchText);
}