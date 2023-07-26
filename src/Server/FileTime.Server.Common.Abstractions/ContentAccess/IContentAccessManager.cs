using FileTime.Core.ContentAccess;

namespace FileTime.Server.Common.ContentAccess;

public interface IContentAccessManager
{
    void AddContentWriter(string transactionId, IContentWriter contentWriter);
    IContentWriter GetContentWriter(string transactionId);
    void RemoveContentWriter(string transactionId);
}