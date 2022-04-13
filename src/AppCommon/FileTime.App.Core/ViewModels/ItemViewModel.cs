using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using MvvmGen;

namespace FileTime.App.Core.ViewModels
{
    [ViewModel]
    [Inject(typeof(IAppState), "_appState")]
    [Inject(typeof(IItemNameConverterService), "_itemNameConverterService")]
    public abstract partial class ItemViewModel : IItemViewModel
    {
        [Property]
        private IItem? _baseItem;

        [Property]
        private IObservable<IReadOnlyList<ItemNamePart>>? _displayName;

        [Property]
        private string? _displayNameText;

        [Property]
        private IObservable<bool>? _isSelected;

        [Property]
        private IObservable<bool>? _isMarked;

        [Property]
        private IObservable<ItemViewMode> _viewMode;

        [Property]
        private DateTime? _createdAt;

        [Property]
        private string? _attributes;

        [Property]
        private BehaviorSubject<bool> _isAlternative = new(false);

        public void Init(IItem item, ITabViewModel parentTab, int index)
        {
            BaseItem = item;
            DisplayName = _appState.SearchText.Select(s => _itemNameConverterService.GetDisplayName(item.DisplayName, s));
            DisplayNameText = item.DisplayName;
            IsMarked = parentTab.MarkedItems.Select(m => m.Contains(item.FullName));
            IsSelected = parentTab.CurrentSelectedItem.Select(i => i == this);
            IsAlternative.OnNext(index % 2 == 0);
            ViewMode = Observable.CombineLatest(IsMarked, IsSelected, IsAlternative, GenerateViewMode);
            Attributes = item.Attributes;
            CreatedAt = item.CreatedAt;
        }

        private ItemViewMode GenerateViewMode(bool isMarked, bool isSelected, bool sAlternative)
        => (isMarked, isSelected, sAlternative) switch
        {
            (true, true, _) => ItemViewMode.MarkedSelected,
            (true, false, true) => ItemViewMode.MarkedAlternative,
            (false, true, _) => ItemViewMode.Selected,
            (false, false, true) => ItemViewMode.Alternative,
            (true, false, false) => ItemViewMode.Marked,
            _ => ItemViewMode.Default
        };
    }
}