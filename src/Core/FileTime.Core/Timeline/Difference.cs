using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.Timeline
{
    public class Difference
    {
        public DifferenceItemType Type { get; }
        public string Name { get; }
        public AbsolutePath AbsolutePath { get; }
        public DifferenceActionType Action { get; }

        public Difference(DifferenceItemType type, DifferenceActionType action, AbsolutePath absolutePath)
        {
            Type = type;
            AbsolutePath = absolutePath;
            Action = action;

            Name = absolutePath.GetName();
        }

        public Difference WithVirtualContentProvider(IContentProvider? virtualContentProvider)
        {
            return new Difference(
                Type,
                Action,
                new AbsolutePath(AbsolutePath.ContentProvider, AbsolutePath.Path, virtualContentProvider)
            );
        }
    }
}