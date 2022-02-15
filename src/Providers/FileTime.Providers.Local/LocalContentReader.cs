using FileTime.Core.Providers;

namespace FileTime.Providers.Local
{
    public class LocalContentReader : IContentReader
    {
        private readonly FileStream _readerStream;
        private readonly BinaryReader _binaryReader;
        private bool disposed;

        public int PreferredBufferSize => 1024 * 1024;

        public LocalContentReader(FileStream readerStream)
        {
            _readerStream = readerStream;
            _binaryReader = new BinaryReader(_readerStream);
        }

        public Task<byte[]> ReadBytesAsync(int bufferSize)
        {
            var max = bufferSize > 0 && bufferSize < PreferredBufferSize ? bufferSize : PreferredBufferSize;

            return Task.FromResult(_binaryReader.ReadBytes(max));
        }

        ~LocalContentReader()
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
            if (!disposed)
            {
                if (disposing)
                {
                    _readerStream.Dispose();
                    _binaryReader.Dispose();
                }
            }
            disposed = true;
        }
    }
}