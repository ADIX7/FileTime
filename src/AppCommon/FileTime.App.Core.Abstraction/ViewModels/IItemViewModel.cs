using System.Reactive.Subjects;
using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;

namespace FileTime.App.Core.ViewModels
{
    public interface IItemViewModel
    {
        IItem? BaseItem { get; set; }
        IObservable<IReadOnlyList<ItemNamePart>>? DisplayName { get; set; }
        IObservable<bool>? IsSelected { get; set; }
        IObservable<bool>? IsMarked { get; set; }
        BehaviorSubject<bool> IsAlternative { get; }
        IObservable<ItemViewMode> ViewMode { get; set; }
        DateTime? CreatedAt { get; set; }
        string? Attributes { get; set; }
    }
}