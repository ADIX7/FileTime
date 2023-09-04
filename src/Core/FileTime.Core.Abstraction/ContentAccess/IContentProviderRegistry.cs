using System.Collections.ObjectModel;
using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

public interface IContentProviderRegistry
{
    ReadOnlyObservableCollection<IContentProvider> ContentProviders { get; }
    ReadOnlyObservableCollection<ISubContentProvider> SubContentProviders { get; }
    void AddContentProvider(IContentProvider contentProvider);
    void RemoveContentProvider(IContentProvider contentProvider);
    Task<ISubContentProvider?> GetSubContentProviderForElement(IElement parentElement);
}