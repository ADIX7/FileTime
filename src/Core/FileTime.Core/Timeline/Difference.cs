using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.Timeline
{
    public class Difference
    {
        public string Name { get; }
        public AbsolutePath AbsolutePath { get; }
        public DifferenceActionType Action { get; }

        public Difference(DifferenceActionType action, AbsolutePath absolutePath)
        {
            AbsolutePath = absolutePath;
            Action = action;

            Name = absolutePath.GetName();
        }

        public Difference WithVirtualContentProvider(IContentProvider? virtualContentProvider)
        {
            return new Difference(
                Action,
                new AbsolutePath(AbsolutePath.ContentProvider, AbsolutePath.Path, AbsolutePath.Type, virtualContentProvider)
            );
        }
    }
}