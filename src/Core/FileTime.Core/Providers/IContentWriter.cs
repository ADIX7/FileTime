namespace FileTime.Core.Providers
{
    public interface IContentWriter : IDisposable
    {
        int PreferredBufferSize { get; }

        Task WriteBytesAsync(byte[] data, int? index = null);
        Task FlushAsync();
    }
}