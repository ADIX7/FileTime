using FileTime.Core.Models;

namespace FileTime.App.Search;

public interface ISearchTask
{
    IContainer SearchContainer { get; }
    Task StartAsync();
}