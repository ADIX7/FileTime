using FileTime.Core.Models;

namespace FileTime.Server.Common.ItemTracker;

public interface IItemTrackerRegistry
{
    int Register(IItem item);
    event Action<int>? ItemRemoved;
}