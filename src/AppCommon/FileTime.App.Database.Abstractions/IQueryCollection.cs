using System.Linq.Expressions;

namespace FileTime.App.Database;

public interface IQueryCollection<T>
{
    IQueryable<T> Query();
    bool Exists(Expression<Func<T, bool>> predicate);
    T? FirstOrDefault(Expression<Func<T, bool>> predicate);
    IEnumerable<T> ToEnumerable();
}