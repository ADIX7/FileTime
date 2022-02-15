using FileTime.Core.Providers;

namespace FileTime.Providers.Smb
{
    public class SmbContentWriter : IContentWriter
    {
        public int PreferredBufferSize => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task FlushAsync()
        {
            throw new NotImplementedException();
        }

        public Task WriteBytesAsync(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}