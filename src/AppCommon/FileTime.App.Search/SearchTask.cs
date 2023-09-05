using System.Collections.ObjectModel;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Models.Extensions;
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
    private readonly Dictionary<FullName, FullName> _realFullNames = new();
    public IReadOnlyDictionary<FullName, FullName> RealFullNames { get; }

    public IContainer SearchContainer => _container;
    public ISearchMatcher Matcher => _matcher;

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
        RealFullNames = _realFullNames.AsReadOnly();

        var extensions = new ExtensionCollection
        {
            new SearchExtension(this),
            new RealContainerProviderExtension(() => new AbsolutePath(_timelessContentProvider, baseContainer))
        };
        _container = new Container(
            baseContainer.Name,
            baseContainer.DisplayName,
            new FullName(randomId),
            new NativePath(randomId),
            parent,
            false,
            true,
            null,
            null,
            SupportsDelete.False,
            false,
            null,
            contentProvider,
            false,
            PointInTime.Present,
            _exceptions,
            new ReadOnlyExtensionCollection(extensions),
            _items
        );
    }

    public async Task StartAsync()
    {
        await _searchingLock.WaitAsync();
        if (_isSearching) return;
        _isSearching = true;
        _searchingLock.Release();

#pragma warning disable CS4014
        Task.Run(BootstrapSearch);
#pragma warning restore CS4014

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
            var item = await itemPath.ResolveAsync();
            if (await _matcher.IsItemMatchAsync(item))
            {
                var childName = _container.FullName.GetChild(itemPath.Path.GetName());
                _realFullNames.Add(childName, itemPath.Path);
                _items.Add(new AbsolutePath(
                    _timelessContentProvider,
                    PointInTime.Present,
                    childName,
                    AbsolutePathType.Container
                ));
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