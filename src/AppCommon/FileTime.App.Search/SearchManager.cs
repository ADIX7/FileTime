using FileTime.Core.Models;

namespace FileTime.App.Search;

public class SearchManager : ISearchManager
{
    private readonly ISearchContentProvider _searchContainerProvider;
    private readonly List<SearchTask> _searchTasks = new();

    public SearchManager(ISearchContentProvider searchContainerProvider)
    {
        _searchContainerProvider = searchContainerProvider;
    }

    public async Task StartSearchAsync(ISearchMatcher matcher, IContainer searchIn)
    {
        var searchTask = new SearchTask(
            searchIn,
            _searchContainerProvider,
            matcher
        );

        _searchTasks.Add(searchTask);

        await searchTask.StartAsync();
    }
}