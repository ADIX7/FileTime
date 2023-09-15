namespace FileTime.Core.ContentAccess;

public interface IStreamContainer : IDisposable
{
    Stream GetStream();
}