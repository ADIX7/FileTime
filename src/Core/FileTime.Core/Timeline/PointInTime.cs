using System.Collections.ObjectModel;
using FileTime.Core.Providers;

namespace FileTime.Core.Timeline
{
    public class PointInTime
    {
        private readonly Dictionary<IContentProvider, RootSnapshot> snapshots = new();

        public IReadOnlyDictionary<IContentProvider, RootSnapshot> Snapshots => new Lazy<IReadOnlyDictionary<IContentProvider, RootSnapshot>>(() => new ReadOnlyDictionary<IContentProvider, RootSnapshot>(snapshots)).Value;
    }
}