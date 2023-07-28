using System.Collections.ObjectModel;
using DynamicData;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Search;

public class SearchTask : ISearchTask
{
    private readonly IContainer _baseContainer;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ISearchMatcher _matcher;
    private readonly Container _container;
    private readonly ObservableCollection<Exception> _exceptions = new();
    private readonly ObservableCollection<AbsolutePath> _items = new();
    private readonly SemaphoreSlim _searchingLock = new(1, 1);
    private bool _isSearching;
    private static int _searchId = 1;

    public IContainer SearchContainer => _container;

    public SearchTask(
        IContainer baseContainer,
        ISearchContentProvider contentProvider,
        ITimelessContentProvider timelessContentProvider,
        ISearchMatcher matcher,
        AbsolutePath parent
    )
    {
        var randomId = $"{SearchContentProvider.ContentProviderName}/{_searchId++}_{baseContainer.Name}";
        _baseContainer = baseContainer;
        _timelessContentProvider = timelessContentProvider;
        _matcher = matcher;
        _container = new Container(
            baseContainer.Name,
            baseContainer.DisplayName,
            new FullName(randomId),
            new NativePath(randomId),
            parent,
            false,
            true,
            null,
            SupportsDelete.False,
            false,
            null,
            contentProvider,
            false,
            PointInTime.Present,
            _exceptions,
            new ReadOnlyExtensionCollection(new ExtensionCollection()),
            _items
        );
    }

    public async Task StartAsync()
    {
        await _searchingLock.WaitAsync();
        if (_isSearching) return;
        _isSearching = true;
        _searchingLock.Release();

        Task.Run(BootstrapSearch);

        async Task BootstrapSearch()
        {
            try
            {
                _container.StartLoading();
                await TraverseTree(_baseContainer);
            }
            finally
            {
                _container.StopLoading();

                await _searchingLock.WaitAsync();
                _isSearching = false;
                _searchingLock.Release();
            }
        }
    }

    private async Task TraverseTree(IContainer container)
    {
        var items = container.Items.ToList();

        var childContainers = new List<IContainer>();

        foreach (var itemPath in items)
        {
            var item = await itemPath.ResolveAsync(
                itemInitializationSettings: new ItemInitializationSettings
                {
                    Parent = new AbsolutePath(_timelessContentProvider, _container)
                });
            if (await _matcher.IsItemMatchAsync(item))
            {
                _items.Add(itemPath);
            }

            if (item is IContainer childContainer)
                childContainers.Add(childContainer);
        }

        foreach (var childContainer in childContainers)
        {
            await TraverseTree(childContainer);
        }
    }
}