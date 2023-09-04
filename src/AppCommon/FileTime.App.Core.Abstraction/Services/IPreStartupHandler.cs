namespace FileTime.App.Core.Services;

public interface IPreStartupHandler
{
    Task InitAsync();
}