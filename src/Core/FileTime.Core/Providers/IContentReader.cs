namespace FileTime.Core.Providers
{
    public interface IContentReader : IDisposable
    {
        int PreferredBufferSize { get; }

        Task<byte[]> ReadBytesAsync(int bufferSize, int? offset = null);
    }
}