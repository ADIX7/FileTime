using System.ComponentModel;
using System.Reactive.Linq;
using DynamicData;
using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Helper;
using FileTime.Core.Models;
using MoreLinq;
using MvvmGen;

namespace FileTime.App.Core.ViewModels;

[ViewModel]
[Inject(typeof(IAppState), "_appState")]
[Inject(typeof(IItemNameConverterService), "_itemNameConverterService")]
public abstract partial class ItemViewModel : IItemViewModel
{
    private ITabViewModel? _parentTab;

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
    private IObservable<bool> _isAlternative;

    public void Init(IItem item, ITabViewModel parentTab, ItemViewModelType itemViewModelType)
    {
        _parentTab = parentTab;

        var sourceCollection = itemViewModelType switch
        {
            ItemViewModelType.Main => parentTab.CurrentItemsCollectionObservable,
            ItemViewModelType.Parent => parentTab.ParentsChildrenCollectionObservable,
            ItemViewModelType.SelectedChild => parentTab.SelectedsChildrenCollectionObservable,
            _ => throw new InvalidEnumArgumentException()
        };

        BaseItem = item;
        DisplayName = _appState.SearchText.Select(s => _itemNameConverterService.GetDisplayName(item.DisplayName, s));
        DisplayNameText = item.DisplayName;

        IsMarked = itemViewModelType is ItemViewModelType.Main
            ? parentTab.MarkedItems.ToCollection().Select(m => m.Any(i => i.Path == item.FullName?.Path))
            : Observable.Return(false);

        IsSelected = itemViewModelType is ItemViewModelType.Main
            ? parentTab.CurrentSelectedItem.Select(EqualsTo)
            : Observable.Return(IsInDeespestPath());

        IsAlternative = sourceCollection.Select(c => c?.Index().FirstOrDefault(i => EqualsTo(i.Value)).Key % 2 == 0);

        ViewMode = Observable.CombineLatest(IsMarked, IsSelected, IsAlternative, GenerateViewMode).Throttle(TimeSpan.FromMilliseconds(10));
        Attributes = item.Attributes;
        CreatedAt = item.CreatedAt;
    }

    private ItemViewMode GenerateViewMode(bool isMarked, bool isSelected, bool isAlternative)
        => (isMarked, isSelected, isAlternative) switch
        {
            (true, true, _) => ItemViewMode.MarkedSelected,
            (true, false, true) => ItemViewMode.MarkedAlternative,
            (false, true, _) => ItemViewMode.Selected,
            (false, false, true) => ItemViewMode.Alternative,
            (true, false, false) => ItemViewMode.Marked,
            _ => ItemViewMode.Default
        };

    public bool EqualsTo(IItemViewModel? itemViewModel)
    {
        return BaseItem?.FullName?.Path is string path && path == itemViewModel?.BaseItem?.FullName?.Path;
    }

    private bool IsInDeespestPath()
    {
        if (_parentTab?.Tab?.LastDeepestSelectedPath is null
            || BaseItem?.FullName is null)
        {
            return false;
        }

        var ownFullName = BaseItem.FullName;
        var deepestPath = _parentTab.Tab.LastDeepestSelectedPath;
        var commonPath = new FullName(PathHelper.GetCommonPath(ownFullName.Path, deepestPath.Path));

        return commonPath.Path == ownFullName.Path;
    }
}