using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Search;

public class SearchContentProvider : ContentProviderBase, ISearchContentProvider
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly List<SearchTask> _searchTasks = new();
    public const string ContentProviderName = "search";

    public SearchContentProvider(ITimelessContentProvider timelessContentProvider)
        : base(ContentProviderName, timelessContentProvider)
    {
        _timelessContentProvider = timelessContentProvider;
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

    public override NativePath GetNativePath(FullName fullName) => new(fullName.Path);
    public override FullName GetFullName(NativePath nativePath) => new(nativePath.Path);

    public override Task<byte[]?> GetContentAsync(
        IElement element,
        int? maxLength = null,
        CancellationToken cancellationToken = default
    )
        => Task.FromResult(null as byte[]);

    public override bool CanHandlePath(NativePath path) => path.Path.StartsWith(ContentProviderName);

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
}