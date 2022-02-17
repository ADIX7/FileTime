using FileTime.Core.Models;
using FileTime.Core.Providers;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace FileTime.Tools.Compression.Command.OperationHandlers
{
    public class CompressOperation<TEntry, TVolume> : ICompressOperation
        where TEntry : IArchiveEntry
        where TVolume : IVolume
    {
        private readonly AbstractWritableArchive<TEntry, TVolume> _archive;
        private readonly Action<Stream> _saveTo;
        private bool _disposed;

        public CompressOperation(AbstractWritableArchive<TEntry, TVolume> archive, Action<Stream> saveTo)
        {
            _archive = archive;
            _saveTo = saveTo;
        }
        public async Task<IEnumerable<IDisposable>> CompressElement(IElement element, string key)
        {
            if (element.Provider.SupportsContentStreams)
            {
                var contentReader = await element.GetContentReaderAsync();
                var contentReaderStream = new ContentProviderStream(contentReader);

                _archive.AddEntry(key, contentReaderStream);

                return new IDisposable[] { contentReader, contentReaderStream };
            }

            return Enumerable.Empty<IDisposable>();
        }

        public void SaveTo(Stream stream)
        {
            _saveTo(stream);
        }

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
}