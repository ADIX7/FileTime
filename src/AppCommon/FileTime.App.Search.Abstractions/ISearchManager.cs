using FileTime.Core.Models;

namespace FileTime.App.Search;

public interface ISearchManager
{
    Task StartSearchAsync(ISearchMatcher matcher, IContainer searchIn);
}