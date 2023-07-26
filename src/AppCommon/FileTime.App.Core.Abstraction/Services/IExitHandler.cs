namespace FileTime.App.Core.Services;

public interface IExitHandler
{
    Task ExitAsync(CancellationToken token = default);
}