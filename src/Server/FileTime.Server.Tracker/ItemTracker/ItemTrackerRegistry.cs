using FileTime.Core.Models;
using FileTime.Server.Common.ItemTracker;

namespace FileTime.Server.Tracker.ItemTracker;

public class ItemTrackerRegistry : IItemTrackerRegistry
{
    private readonly object _lock = new();
    private readonly Dictionary<int, WeakReference<IItem>> _items = new();

    private int _globalId = 1;
    public event Action<int>? ItemRemoved;

    public int Register(IItem item)
    {
        lock (_lock)
        {
            while (_items.ContainsKey(_globalId)) _globalId++;
            _items[_globalId] = new WeakReference<IItem>(item);
            return _globalId;
        }
    }

    private void Clean()
    {
        lock (_lock)
        {
            var keys = _items.Keys.ToArray();
            var keysToRemove = new List<int>();
            foreach (var key in keys)
            {
                if (!_items[key].TryGetTarget(out _))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _items.Remove(key);
                ItemRemoved?.Invoke(key);
            }
        }
    }
}