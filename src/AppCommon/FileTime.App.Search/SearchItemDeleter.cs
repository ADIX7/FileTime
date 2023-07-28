using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.App.Search;

public class SearchItemDeleter : IItemDeleter<ISearchContentProvider>
{
    public Task DeleteAsync(ISearchContentProvider contentProvider, FullName fullName)
    {
        contentProvider.RemoveSearch(fullName);
        return Task.CompletedTask;
    }
}