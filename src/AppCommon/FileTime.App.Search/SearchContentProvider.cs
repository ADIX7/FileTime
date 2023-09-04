using FileTime.App.Core.Exceptions;
using FileTime.Core.Behaviors;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Search;

public class SearchContentProvider : ContentProviderBase, ISearchContentProvider, IItemNameConverterProvider
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly List<SearchTask> _searchTasks = new();
    public const string ContentProviderName = "search";

    public SearchContentProvider(ITimelessContentProvider timelessContentProvider)
        : base(ContentProviderName, timelessContentProvider)
    {
        _timelessContentProvider = timelessContentProvider;
    }

    public override async Task<IItem> GetItemByFullNameAsync(
        FullName fullName,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default
    )
    {
        if (fullName.Path == ContentProviderName)
            return this;

        if (_searchTasks
                .FirstOrDefault(t => t.SearchContainer.FullName == fullName) is { } searchTask)
        {
            return searchTask.SearchContainer;
        }

        if (_searchTasks.FirstOrDefault(t => t.RealFullNames.ContainsKey(fullName)) is { } searchTask2)
        {
            var realFullName = searchTask2.RealFullNames[fullName];
            var item = await _timelessContentProvider.GetItemByFullNameAsync(
                realFullName,
                pointInTime,
                forceResolve,
                forceResolvePathType,
                itemInitializationSettings
            );
            item = item.WithParent(new AbsolutePath(_timelessContentProvider, searchTask2.SearchContainer));
            return item;
        }

        throw new ItemNotFoundException(fullName);
    }

    public override Task<IItem> GetItemByNativePathAsync(
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default
    )
    {
        if (nativePath.Path == ContentProviderName) return Task.FromResult((IItem) this);
        return Task.FromResult((IItem) _searchTasks
            .First(searchTask => searchTask.SearchContainer.NativePath == nativePath).SearchContainer);
    }

    public override ValueTask<NativePath> GetNativePathAsync(FullName fullName)
        => ValueTask.FromResult(new NativePath(fullName.Path));

    public override FullName GetFullName(NativePath nativePath) => new(nativePath.Path);

    public override Task<byte[]?> GetContentAsync(
        IElement element,
        int? maxLength = null,
        CancellationToken cancellationToken = default
    )
        => Task.FromResult(null as byte[]);

    public override Task<bool> CanHandlePathAsync(NativePath path) => Task.FromResult(path.Path.StartsWith(ContentProviderName));
    public override VolumeSizeInfo? GetVolumeSizeInfo(FullName path) => null;

    public async Task<ISearchTask> StartSearchAsync(ISearchMatcher matcher, IContainer searchIn)
    {
        var searchTask = new SearchTask(
            searchIn,
            this,
            _timelessContentProvider,
            matcher,
            new AbsolutePath(_timelessContentProvider, this)
        );

        _searchTasks.Add(searchTask);
        await searchTask.StartAsync();
        Items.Add(new AbsolutePath(_timelessContentProvider, searchTask.SearchContainer));

        return searchTask;
    }

    public void RemoveSearch(FullName searchFullName)
    {
        var searchTask = _searchTasks.FirstOrDefault(t => t.SearchContainer.FullName == searchFullName);
        if (searchTask is null) return;

        _searchTasks.Remove(searchTask);
        var searchItem = Items.FirstOrDefault(c => c.Path == searchTask.SearchContainer.FullName);
        if (searchItem is not null)
        {
            Items.Remove(searchItem);
        }
    }

    public async Task<IEnumerable<ItemNamePart>> GetItemNamePartsAsync(IItem item)
    {
        var currentItem = item;
        SearchTask? searchTask = null;

        while (searchTask is null && currentItem is not null)
        {
            searchTask = currentItem.GetExtension<SearchExtension>()?.SearchTask;

            currentItem = currentItem.Parent is null
                ? null
                : await currentItem.Parent.ResolveAsync(itemInitializationSettings: new ItemInitializationSettings {SkipChildInitialization = true});
        }

        if (searchTask is null) return new List<ItemNamePart> {new(item.DisplayName)};

        return searchTask.Matcher.GetDisplayName(item);
    }
}