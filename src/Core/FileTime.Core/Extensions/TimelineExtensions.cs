using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Extensions
{
    public static class TimelineExtensions
    {
        public static DifferenceItemType ToDifferenceItemType(this IItem? item)
        {
            if (item is IContainer) return DifferenceItemType.Container;
            else if (item is IElement) return DifferenceItemType.Element;
            else return DifferenceItemType.Unknown;
        }
    }
}