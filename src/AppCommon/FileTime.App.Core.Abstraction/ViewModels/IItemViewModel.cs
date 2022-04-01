using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;

namespace FileTime.App.Core.ViewModels
{
    public interface IItemViewModel
    {
        IItem? Item { get; set; }
        IObservable<IReadOnlyList<ItemNamePart>>? DisplayName { get; set; }
        IObservable<bool>? IsSelected { get; set; }
        IObservable<bool>? IsMarked { get; set; }
        ItemViewMode ViewMode { get; set; }
    }
}