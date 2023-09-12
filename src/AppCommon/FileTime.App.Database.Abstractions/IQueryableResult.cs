namespace FileTime.App.Database;

public interface IQueryableResult<T>
{
    int Count();
    bool Exists();
    T First();
    T? FirstOrDefault();
    T Single();
    T? SingleOrDefault();
    IEnumerable<T> ToEnumerable();
    List<T> ToList();
    T[] ToArray();
}