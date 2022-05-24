namespace FileTime.App.Core.Services.Persistence;

public interface ITabPersistenceService : IStartupHandler, IExitHandler
{
    void SaveStates();
}