using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FileTime.Core.Models;

public class ExtensionCollection : IEnumerable<object>
{
    private readonly List<object> _extensions = new();

    public ExtensionCollection()
    {
    }

    public ExtensionCollection(IEnumerable<object> objects)
    {
        foreach (var obj in objects)
        {
            AddSafe(obj);
        }
    }

    private void AddSafe(object obj)
    {
        var objType = obj.GetType();
        if (_extensions.Any(i => i.GetType() == objType))
            throw new ArgumentException($"Collection already contains an item with type {objType.FullName}");

        _extensions.Add(obj);
    }

    public void Add<T>([DisallowNull] T obj, [CallerArgumentExpression("obj")] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(obj, paramName);
        AddSafe(obj);
    }

    public void Remove<T>()
    {
        _extensions.RemoveAll(i => i is T);
    }

    public ReadOnlyExtensionCollection AsReadOnly() => new ReadOnlyExtensionCollection(this);

    public IEnumerator<object> GetEnumerator() => _extensions.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _extensions.GetEnumerator();
}