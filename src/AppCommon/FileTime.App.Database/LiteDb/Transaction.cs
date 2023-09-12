using LiteDB;

namespace FileTime.App.Database.LiteDb;

public class Transaction : ITransaction
{
    private readonly ILiteDatabase _liteDatabase;

    public Transaction(ILiteDatabase liteDatabase)
    {
        _liteDatabase = liteDatabase;
    }

    public ValueTask CommitAsync()
    {
        _liteDatabase.Commit();
        return ValueTask.CompletedTask;
    }

    public void Rollback() => _liteDatabase.Rollback();

    public IUpdatable<T> GetCollection<T>(string collectionName) => new Updatable<T>(_liteDatabase.GetCollection<T>(collectionName));

    public void Dispose() => _liteDatabase.Dispose();
}