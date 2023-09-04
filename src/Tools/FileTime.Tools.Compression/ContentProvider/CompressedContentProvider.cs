using System.Text;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Tools.Compression.ContentProvider;

public class CompressedContentProvider : SubContentProviderBase, ICompressedContentProvider
{
    public CompressedContentProvider(
        IContentProvider parentContentProvider,
        ITimelessContentProvider timelessContentProvider
    )
        : base(parentContentProvider, "compression", timelessContentProvider)
    {
    }

    public override Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default) 
        => Task.FromResult((byte[]?)"Not implemented..."u8.ToArray());

    public override VolumeSizeInfo? GetVolumeSizeInfo(FullName path) => throw new NotImplementedException();
}