using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace FileTime.Tools.Compression;

public class CompressOperation<TEntry, TVolume>(IContentAccessorFactory contentAccessorFactory,
        AbstractWritableArchive<TEntry, TVolume> archive,
        Action<Stream> saveTo)
    : ICompressOperation
    where TEntry : IArchiveEntry
    where TVolume : IVolume
{
    private bool _disposed;

    public async Task<IEnumerable<IDisposable>> CompressElement(IElement element, string key)
    {
        var contentReader = await contentAccessorFactory.GetContentReaderFactory(element.Provider).CreateContentReaderAsync(element);
        var contentReaderStream = contentReader.GetStream();
        archive.AddEntry(key, contentReaderStream);

        return new IDisposable[] {contentReader, contentReaderStream};
    }

    public void SaveTo(Stream stream)
        => saveTo(stream);

    ~CompressOperation()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                archive.Dispose();
            }
        }

        _disposed = true;
    }
}