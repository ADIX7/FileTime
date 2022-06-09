namespace FileTime.Core.ContentAccess;

public interface IContentWriter : IDisposable
{
    int PreferredBufferSize { get; }

    Task WriteBytesAsync(byte[] data, int? index = null);
    Task FlushAsync();
}