using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.ContentAccess;

public abstract class SubContentProviderBase : ContentProviderBase
{
    private readonly IContentAccessorFactory _contentAccessorFactory;

    public IContentProvider ParentContentProvider { get; }

    protected SubContentProviderBase(
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory,
        IContentProvider parentContentProvider,
        string name
    ) : base(name, timelessContentProvider)
    {
        _contentAccessorFactory = contentAccessorFactory;
        ParentContentProvider = parentContentProvider;
    }

    public override async Task<IItem> GetItemByNativePathAsync(
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
        => await ParentContentProvider.GetItemByNativePathAsync(
            nativePath,
            pointInTime,
            forceResolve,
            forceResolvePathType,
            itemInitializationSettings);

    public override async ValueTask<NativePath> GetNativePathAsync(FullName fullName)
        => await ParentContentProvider.GetNativePathAsync(fullName);

    public override FullName GetFullName(NativePath nativePath)
        => ParentContentProvider.GetFullName(nativePath);

    public override async Task<bool> CanHandlePathAsync(NativePath path)
        => await ParentContentProvider.CanHandlePathAsync(path);

    public override async ValueTask<NativePath?> GetSupportedPathPart(NativePath nativePath)
        => await ParentContentProvider.GetSupportedPathPart(nativePath);

    protected async Task<ParentElementReaderContext> GetParentElementReaderAsync(IItem item)
        => await Helper.GetParentElementReaderAsync(_contentAccessorFactory, item, ParentContentProvider);

    protected async Task<IElement> GetParentElementAsync(IItem item)
        => await Helper.GetParentElementAsync(item, ParentContentProvider);
}