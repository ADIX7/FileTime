namespace FileTime.App.Core.Services;

public interface ILifecycleService
{
    Task InitStartupHandlersAsync();
    Task ExitAsync();
}