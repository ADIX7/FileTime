using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace FileTime.Tools.Compression;

public class CompressOperation<TEntry, TVolume> : ICompressOperation
    where TEntry : IArchiveEntry
    where TVolume : IVolume
{
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly AbstractWritableArchive<TEntry, TVolume> _archive;
    private readonly Action<Stream> _saveTo;
    private bool _disposed;

    public CompressOperation(
        IContentAccessorFactory contentAccessorFactory,
        AbstractWritableArchive<TEntry, TVolume> archive,
        Action<Stream> saveTo
    )
    {
        _contentAccessorFactory = contentAccessorFactory;
        _archive = archive;
        _saveTo = saveTo;
    }

    public async Task<IEnumerable<IDisposable>> CompressElement(IElement element, string key)
    {
        if (element.Provider.SupportsContentStreams)
        {
            var contentReader = await _contentAccessorFactory.GetContentReaderFactory(element.Provider).CreateContentReaderAsync(element);

            var contentReaderStream = contentReader.AsStream();

            _archive.AddEntry(key, contentReaderStream);

            return new IDisposable[] {contentReader, contentReaderStream};
        }

        return Enumerable.Empty<IDisposable>();
    }

    public void SaveTo(Stream stream) 
        => _saveTo(stream);

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
                _archive.Dispose();
            }
        }

        _disposed = true;
    }
}