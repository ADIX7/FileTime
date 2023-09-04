using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.ContentAccess;

public abstract class SubContentProviderBase : ContentProviderBase
{
    private readonly IContentProvider _parentContentProvider;

    protected SubContentProviderBase(
        IContentProvider parentContentProvider,
        string name,
        ITimelessContentProvider timelessContentProvider) : base(name, timelessContentProvider)
    {
        _parentContentProvider = parentContentProvider;
    }

    public override async Task<IItem> GetItemByNativePathAsync(
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
        => await _parentContentProvider.GetItemByNativePathAsync(
            nativePath,
            pointInTime,
            forceResolve,
            forceResolvePathType,
            itemInitializationSettings);

    public override async ValueTask<NativePath> GetNativePathAsync(FullName fullName)
        => await _parentContentProvider.GetNativePathAsync(fullName);

    public override FullName GetFullName(NativePath nativePath)
        => _parentContentProvider.GetFullName(nativePath);

    public override async Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default)
        => await _parentContentProvider.GetContentAsync(element, maxLength, cancellationToken);

    public override async Task<bool> CanHandlePathAsync(NativePath path)
        => await _parentContentProvider.CanHandlePathAsync(path);

    public override VolumeSizeInfo? GetVolumeSizeInfo(FullName path)
        => _parentContentProvider.GetVolumeSizeInfo(path);
}