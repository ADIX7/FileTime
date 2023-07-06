using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Search;

public class SearchContentProvider : ContentProviderBase, ISearchContentProvider
{
    private readonly ISearchManager _searchManager;
    public const string ContentProviderName = "search";

    public SearchContentProvider(ISearchManager searchManager) : base(ContentProviderName)
    {
        _searchManager = searchManager;
    }

    public override Task<IItem> GetItemByNativePathAsync(
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default
    ) =>
        Task.FromResult((IItem) _searchManager.SearchTasks
            .First(searchTask => searchTask.SearchContainer.NativePath == nativePath).SearchContainer);

    public override NativePath GetNativePath(FullName fullName) => new(fullName.Path);
    public override FullName GetFullName(NativePath nativePath) => new(nativePath.Path);

    public override Task<byte[]?> GetContentAsync(
        IElement element,
        int? maxLength = null,
        CancellationToken cancellationToken = default
    )
        => Task.FromResult(null as byte[]);

    public override bool CanHandlePath(NativePath path) => path.Path.StartsWith(ContentProviderName);
}