using System.Threading.Tasks;
using FileTime.Core.Providers;

namespace FileTime.Providers.Local
{
    public class LocalContentReader : IContentReader
    {
        private readonly FileStream _readerStream;
        private readonly BinaryReader _binaryReader;
        private bool disposed;

        public int PreferredBufferSize => 1024 * 1024;
        private long? _bytesRead;

        public LocalContentReader(FileStream readerStream)
        {
            _readerStream = readerStream;
            _binaryReader = new BinaryReader(_readerStream);
        }

        public Task<byte[]> ReadBytesAsync(int bufferSize, int? offset = null)
        {
            var max = bufferSize > 0 && bufferSize < PreferredBufferSize ? bufferSize : PreferredBufferSize;

            if (offset != null)
            {
                if (_bytesRead == null) _bytesRead = 0;
                var buffer = new byte[max];
                var bytesRead = _binaryReader.Read(buffer, offset.Value, max);
                _bytesRead += bytesRead;

                if (buffer.Length != bytesRead)
                {
                    Array.Resize(ref buffer, bytesRead);
                }
                return Task.FromResult(buffer);
            }
            else
            {
                return Task.FromResult(_binaryReader.ReadBytes(max));
            }
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