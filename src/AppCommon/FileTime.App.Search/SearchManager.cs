using FileTime.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Search;

public class SearchManager : ISearchManager
{
    private readonly IServiceProvider _serviceProvider;
    private ISearchContentProvider? _searchContainerProvider;
    private readonly List<SearchTask> _searchTasks = new();
    
    public IReadOnlyList<ISearchTask> SearchTasks { get; }

    public SearchManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        SearchTasks = _searchTasks.AsReadOnly();
    }

    public async Task<ISearchTask> StartSearchAsync(ISearchMatcher matcher, IContainer searchIn)
    {
        _searchContainerProvider ??= _serviceProvider.GetRequiredService<ISearchContentProvider>();
        var searchTask = new SearchTask(
            searchIn,
            _searchContainerProvider,
            matcher
        );

        _searchTasks.Add(searchTask);

        await searchTask.StartAsync();

        return searchTask;
    }
}