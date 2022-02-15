using FileTime.Core.Providers;
using SMBLibrary;
using SMBLibrary.Client;

namespace FileTime.Providers.Smb
{
    public class SmbContentReader : IContentReader
    {
        private readonly ISMBFileStore _smbFileStore;
        private readonly object _fileHandle;
        private readonly ISMBClient _client;
        private bool disposed;
        private long _bytesRead;

        public int PreferredBufferSize => (int)_client.MaxReadSize;

        public SmbContentReader(ISMBFileStore smbFileStore, object fileHandle, ISMBClient client)
        {
            _smbFileStore = smbFileStore;
            _fileHandle = fileHandle;
            _client = client;
        }

        public Task<byte[]> ReadBytesAsync(int bufferSize)
        {
            var max = bufferSize > 0 && bufferSize < (int)_client.MaxReadSize ? bufferSize : (int)_client.MaxReadSize;

            var status = _smbFileStore.ReadFile(out byte[] data, _fileHandle, _bytesRead, max);
            if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_END_OF_FILE)
            {
                throw new Exception("Failed to read from file");
            }

            if (status == NTStatus.STATUS_END_OF_FILE || data.Length == 0)
            {
                return Task.FromResult(Array.Empty<byte>());
            }
            _bytesRead += data.Length;

            return Task.FromResult(data);
        }

        ~SmbContentReader()
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
                    _smbFileStore.CloseFile(_fileHandle);
                    _smbFileStore.Disconnect();
                }
            }
            disposed = true;
        }
    }
}