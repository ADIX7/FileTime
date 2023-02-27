using FileTime.Core.Models;

namespace FileTime.App.Search;

public interface ISearchManager
{
    Task<ISearchTask> StartSearchAsync(ISearchMatcher matcher, IContainer searchIn);
    IReadOnlyList<ISearchTask> SearchTasks { get; }
}