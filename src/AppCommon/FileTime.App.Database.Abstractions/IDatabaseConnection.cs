namespace FileTime.App.Database;

public interface IDatabaseConnection : IDisposable
{
    ITransaction BeginTransaction();
    IQueryCollection<T> GetCollection<T>(string collectionName);
}