namespace FileTime.App.Database;

public interface ITransaction : IDisposable
{
    ValueTask CommitAsync();
    void Rollback();
    IUpdatable<T> GetCollection<T>(string collectionName);
}