using System.Linq.Expressions;
using LiteDB;

namespace FileTime.App.Database.LiteDb;

public class Queryable<T> : IQueryable<T>
{
    private readonly ILiteQueryable<T> _collection;
    private int SkipCount { get; init; }
    private int TakeCount { get; init; }

    public Queryable(ILiteQueryable<T> collection)
    {
        _collection = collection;
    }

    public IQueryable<T> Where(Expression<Func<T, bool>> predicate) => new Queryable<T>(_collection.Where(predicate));
    public IQueryable<T> OrderBy<TSelector>(Expression<Func<T, TSelector>> order) => new Queryable<T>(_collection.OrderBy(order));
    public IQueryable<T> OrderByDescending<TSelector>(Expression<Func<T, TSelector>> order) => new Queryable<T>(_collection.OrderByDescending(order));
    public IQueryable<T> Skip(int skip) => new Queryable<T>(_collection) {SkipCount = skip};
    public IQueryable<T> Take(int take) => new Queryable<T>(_collection) {TakeCount = take};
    public IQueryable<T> Include<TResult>(Expression<Func<T, TResult>> selector) => new Queryable<T>(_collection.Include(selector));

    private ILiteQueryableResult<TCollection> ApplySkipAndTake<TCollection>(ILiteQueryableResult<TCollection> collection)
    {
        if (SkipCount > 0)
        {
            collection = collection.Skip(SkipCount);
        }

        if (TakeCount > 0)
        {
            collection = collection.Limit(TakeCount);
        }

        return collection;
    }


    public IQueryableResult<TResult> Select<TResult>(Expression<Func<T, TResult>> selector) => new QueryableResult<TResult>(ApplySkipAndTake(_collection.Select(selector)));

    public int Count() => ApplySkipAndTake(_collection).Count();
    public bool Exists() => ApplySkipAndTake(_collection).Exists();
    public T First() => ApplySkipAndTake(_collection).First();
    public T FirstOrDefault() => ApplySkipAndTake(_collection).FirstOrDefault();
    public T Single() => ApplySkipAndTake(_collection).Single();
    public T SingleOrDefault() => ApplySkipAndTake(_collection).SingleOrDefault();
    public IEnumerable<T> ToEnumerable() => ApplySkipAndTake(_collection).ToEnumerable();
    public List<T> ToList() => ApplySkipAndTake(_collection).ToList();
    public T[] ToArray() => ApplySkipAndTake(_collection).ToArray();
}