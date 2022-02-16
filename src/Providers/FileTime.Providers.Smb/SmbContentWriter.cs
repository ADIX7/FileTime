using FileTime.Core.Providers;
using SMBLibrary;
using SMBLibrary.Client;

namespace FileTime.Providers.Smb
{
    public class SmbContentWriter : IContentWriter
    {
        private readonly ISMBFileStore _smbFileStore;
        private readonly object _fileHandle;
        private readonly ISMBClient _client;
        private bool _disposed;
        private int _writeOffset;

        public int PreferredBufferSize => (int)_client.MaxWriteSize;

        public SmbContentWriter(ISMBFileStore smbFileStore, object fileHandle, ISMBClient client)
        {
            _smbFileStore = smbFileStore;
            _fileHandle = fileHandle;
            _client = client;
        }

        public Task FlushAsync()
        {
            return Task.CompletedTask;
        }

        public Task WriteBytesAsync(byte[] data)
        {
            var status = _smbFileStore.WriteFile(out int numberOfBytesWritten, _fileHandle, _writeOffset, data);
            if (status != NTStatus.STATUS_SUCCESS)
            {
                throw new Exception("Failed to write to file");
            }
            _writeOffset += numberOfBytesWritten;

            return Task.CompletedTask;
        }

        ~SmbContentWriter()
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
                    _smbFileStore.CloseFile(_fileHandle);
                    _smbFileStore.Disconnect();
                }
            }
            _disposed = true;
        }
    }
}