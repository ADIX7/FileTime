using FileTime.Core.ContentAccess;

namespace FileTime.Server.Common.ContentAccess;

public interface IContentAccessManager
{
    void AddContentStreamContainer(string transactionId, IStreamContainer streamContainer);
    Stream GetContentStream(string transactionId);
    void RemoveContentStreamContainer(string transactionId);
}