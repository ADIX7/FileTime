using System.Linq.Expressions;

namespace FileTime.App.Database;

public interface IQueryable<T> : IQueryableResult<T>
{
    IQueryable<T> Where(Expression<Func<T, bool>> predicate);
    IQueryable<T> Skip(int skip);
    IQueryable<T> Take(int take);
    IQueryable<T> Include<TResult>(Expression<Func<T, TResult>> selector);
    IQueryableResult<TResult> Select<TResult>(Expression<Func<T, TResult>> selector);
    IQueryable<T> OrderBy<TSelector>(Expression<Func<T, TSelector>> order);
    IQueryable<T> OrderByDescending<TSelector>(Expression<Func<T, TSelector>> order);
}