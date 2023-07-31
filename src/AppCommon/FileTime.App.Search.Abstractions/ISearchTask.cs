using FileTime.Core.Models;

namespace FileTime.App.Search;

public interface ISearchTask
{
    IContainer SearchContainer { get; }
    IReadOnlyDictionary<FullName, FullName> RealFullNames { get; }
    Task StartAsync();
}