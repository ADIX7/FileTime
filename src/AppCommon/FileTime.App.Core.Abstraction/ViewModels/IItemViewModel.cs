using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels
{
    public interface IItemViewModel : IInitable<IItem, ITabViewModel>
    {
        IItem? BaseItem { get; set; }
        IObservable<IReadOnlyList<ItemNamePart>>? DisplayName { get; set; }
        string? DisplayNameText { get; set; }
        IObservable<bool>? IsSelected { get; set; }
        IObservable<bool>? IsMarked { get; set; }
        IObservable<bool> IsAlternative { get; }
        IObservable<ItemViewMode> ViewMode { get; set; }
        DateTime? CreatedAt { get; set; }
        string? Attributes { get; set; }
        bool EqualsTo(IItemViewModel? itemViewModel);
    }
}