using DeclarativeProperty;
using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface IItemViewModel : IInitable<IItem, ITabViewModel, ItemViewModelType>
{
    IItem? BaseItem { get; set; }
    IDeclarativeProperty<IReadOnlyList<ItemNamePart>>? DisplayName { get; }
    string? DisplayNameText { get; set; }
    IDeclarativeProperty<bool> IsSelected { get; set; }
    IObservable<bool>? IsMarked { get; set; }
    IDeclarativeProperty<bool> IsAlternative { get; }
    IObservable<ItemViewMode> ViewMode { get; set; }
    DateTime? CreatedAt { get; set; }
    string? Attributes { get; set; }
    bool EqualsTo(IItemViewModel? itemViewModel);
}