namespace FileTime.Core.ContentAccess;

public interface IContentProviderRegistry
{
    IEnumerable<IContentProvider> ContentProviders { get; }
    void AddContentProvider(IContentProvider contentProvider);
    void RemoveContentProvider(IContentProvider contentProvider);
}