using System.Linq.Expressions;
using LiteDB;

namespace FileTime.App.Database.LiteDb;

public class QueryCollection<T> : IQueryCollection<T>
{
    private readonly ILiteCollection<T> _collection;

    public QueryCollection(ILiteCollection<T> collection)
    {
        _collection = collection;
    }

    public IQueryable<T> Query() => new Queryable<T>(_collection.Query());
    
    public bool Exists(Expression<Func<T, bool>> predicate) => _collection.Exists(predicate);
    public T? FirstOrDefault(Expression<Func<T, bool>> predicate) => _collection.FindOne(predicate);
    public IEnumerable<T> ToEnumerable() => _collection.FindAll();
}