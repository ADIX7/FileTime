using FileTime.Core.ContentAccess;

namespace FileTime.Server.Common.ContentAccess;

public class ContentAccessManager : IContentAccessManager
{
    private readonly Dictionary<string, IStreamContainer> _contentStreamContainers = new();

    public void AddContentStreamContainer(string transactionId, IStreamContainer streamContainer)
        => _contentStreamContainers.Add(transactionId, streamContainer);

    public Stream GetContentStream(string transactionId) => _contentStreamContainers[transactionId].GetStream();

    public void RemoveContentStreamContainer(string transactionId)
    {
        if (!_contentStreamContainers.TryGetValue(transactionId, out var streamContainer)) return;

        streamContainer.Dispose();
        _contentStreamContainers.Remove(transactionId);
    }
}