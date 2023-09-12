using LiteDB;

namespace FileTime.App.Database.LiteDb;

public class QueryableResult<T> : IQueryableResult<T>
{
    private readonly ILiteQueryableResult<T> _queryableResult;

    public QueryableResult(ILiteQueryableResult<T> queryableResult)
    {
        _queryableResult = queryableResult;
    }
    
    public int Count() => _queryableResult.Count();
    public bool Exists() => _queryableResult.Exists();
    public T First() => _queryableResult.First();
    public T FirstOrDefault() => _queryableResult.FirstOrDefault();
    public T Single() => _queryableResult.Single();
    public T SingleOrDefault() => _queryableResult.SingleOrDefault();
    public IEnumerable<T> ToEnumerable() => _queryableResult.ToEnumerable();
    public List<T> ToList() => _queryableResult.ToList();
    public T[] ToArray() => _queryableResult.ToArray();
}