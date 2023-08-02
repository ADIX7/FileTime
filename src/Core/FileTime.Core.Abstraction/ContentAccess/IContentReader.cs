namespace FileTime.Core.ContentAccess;

public interface IContentReader : IDisposable
{
    int PreferredBufferSize { get; }
    long? Position { get; }

    Task<byte[]> ReadBytesAsync(int bufferSize, int? offset = null);
    void SetPosition(long position);
    Stream AsStream();
}