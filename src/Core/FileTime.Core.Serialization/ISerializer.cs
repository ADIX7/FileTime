using FileTime.Core.Models;

namespace FileTime.Core.Serialization;

public interface ISerializer
{
    Task<ISerialized> SerializeAsync(int id, object item);
}

public interface ISerializer<T> where T : IItem
{
    async Task<ISerialized> SerializeAsync(int id, object item) => await SerializeAsync(id, (T) item);
    Task<ISerialized> SerializeAsync(int id, T item);
}