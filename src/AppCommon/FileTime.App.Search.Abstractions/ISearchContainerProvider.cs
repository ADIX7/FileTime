using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.App.Search;

public interface ISearchContentProvider : IContentProvider
{
    Task<ISearchTask> StartSearchAsync(ISearchMatcher matcher, IContainer searchIn);
    void RemoveSearch(FullName searchFullName);
}