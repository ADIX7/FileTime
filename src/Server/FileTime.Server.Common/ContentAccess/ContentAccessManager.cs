using FileTime.Core.ContentAccess;

namespace FileTime.Server.Common.ContentAccess;

public class ContentAccessManager : IContentAccessManager
{
    private readonly Dictionary<string, IContentWriter> _contentWriters = new();
    public void AddContentWriter(string transactionId, IContentWriter contentWriter) 
        => _contentWriters.Add(transactionId, contentWriter);
    
    public IContentWriter GetContentWriter(string transactionId) => _contentWriters[transactionId];
    public void RemoveContentWriter(string transactionId) => _contentWriters.Remove(transactionId);
}