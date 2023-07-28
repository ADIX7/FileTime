using System.Collections.ObjectModel;

namespace FileTime.Core.ContentAccess;

public interface IContentProviderRegistry
{
    ReadOnlyObservableCollection<IContentProvider> ContentProviders { get; }
    void AddContentProvider(IContentProvider contentProvider);
    void RemoveContentProvider(IContentProvider contentProvider);
}