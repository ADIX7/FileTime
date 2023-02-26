using System.Reactive.Linq;
using DynamicData;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Search;

public class SearchTask
{
    private readonly IContainer _baseContainer;
    private readonly ISearchMatcher _matcher;
    private readonly Container _container;
    private readonly SourceList<Exception> _exceptions = new();
    private readonly SourceCache<AbsolutePath, string> _items = new(p => p.Path.Path);
    private readonly SemaphoreSlim _searchingLock = new(1, 1);
    private bool _isSearching;

    public SearchTask(
        IContainer baseContainer,
        IContentProvider contentProvider,
        ISearchMatcher matcher
    )
    {
        _baseContainer = baseContainer;
        _matcher = matcher;
        _container = new Container(
            baseContainer.Name,
            baseContainer.DisplayName,
            new FullName(""),
            new NativePath(""),
            null,
            false,
            true,
            null,
            SupportsDelete.False,
            false,
            null,
            contentProvider,
            false,
            PointInTime.Present,
            _exceptions.Connect(),
            new ReadOnlyExtensionCollection(new ExtensionCollection()),
            Observable.Return(_items.Connect())
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
                _container.IsLoading.OnNext(true);
                await TraverseTree(_baseContainer);
            }
            finally
            {
                _container.IsLoading.OnNext(false);

                await _searchingLock.WaitAsync();
                _isSearching = false;
                _searchingLock.Release();
            }
        }
    }

    private async Task TraverseTree(IContainer container)
    {
        var items = (await container.Items.GetItemsAsync())?.ToList();
        if (items is null) return;

        var childContainers = new List<IContainer>();

        foreach (var itemPath in items)
        {
            var item = await itemPath.ResolveAsync();
            if (await _matcher.IsItemMatchAsync(item))
            {
                _items.AddOrUpdate(itemPath);
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