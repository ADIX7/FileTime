namespace FileTime.App.Database;

public interface IUpdatable<T>
{
    void Insert(T item);
    void Update(T item);
    void Delete(int id);
}