using LiteDB;

namespace FileTime.App.Database.LiteDb;

public class Updatable<T> : IUpdatable<T>
{
    private readonly ILiteCollection<T> _collection;

    public Updatable(ILiteCollection<T> collection)
    {
        _collection = collection;
    }

    public void Insert(T item) => _collection.Insert(item);

    public void Update(T item) => _collection.Update(item);

    public void Delete(int id) => _collection.Delete(new BsonValue(id));
}