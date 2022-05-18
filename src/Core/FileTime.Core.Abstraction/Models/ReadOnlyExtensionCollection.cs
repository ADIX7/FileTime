using System.Collections;

namespace FileTime.Core.Models;

public class ReadOnlyExtensionCollection : IEnumerable<object>
{
    private readonly ExtensionCollection _collection;

    public ReadOnlyExtensionCollection(ExtensionCollection collection)
    {
        _collection = collection;
    }

    public IEnumerator<object> GetEnumerator() => _collection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
}