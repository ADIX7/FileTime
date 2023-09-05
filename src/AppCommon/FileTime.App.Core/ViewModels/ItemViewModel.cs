using System.ComponentModel;
using DeclarativeProperty;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Behaviors;
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

    [Property] private IItem? _baseItem;

    [Property] private string? _displayNameText;

    [Property] private IDeclarativeProperty<bool> _isSelected;

    [Property] private IDeclarativeProperty<bool>? _isMarked;

    [Property] private IDeclarativeProperty<ItemViewMode> _viewMode;

    [Property] private DateTime? _createdAt;
    [Property] private DateTime? _modifiedAt;

    [Property] private string? _attributes;

    [Property] private IDeclarativeProperty<bool> _isAlternative;

    public IDeclarativeProperty<IReadOnlyList<ItemNamePart>>? DisplayName { get; private set; }

    public void Init(
        IItem item,
        ITabViewModel parentTab,
        ItemViewModelType itemViewModelType)
    {
        _parentTab = parentTab;

        var sourceCollection = itemViewModelType switch
        {
            ItemViewModelType.Main => parentTab.CurrentItems!,
            ItemViewModelType.Parent => parentTab.ParentsChildren,
            ItemViewModelType.SelectedChild => parentTab.SelectedsChildren,
            _ => throw new InvalidEnumArgumentException()
        };

        var displayName = itemViewModelType switch
        {
            ItemViewModelType.Main => _appState.RapidTravelTextDebounced.Map(async s =>
                _appState.ViewMode.Value != Models.Enums.ViewMode.RapidTravel
                && _appState.SelectedTab.Value?.CurrentLocation.Value?.Provider is IItemNameConverterProvider nameConverterProvider
                    ? (IReadOnlyList<ItemNamePart>) await nameConverterProvider.GetItemNamePartsAsync(item)
                    : _itemNameConverterService.GetDisplayName(item.DisplayName, s)
            ),
            _ => new DeclarativeProperty<IReadOnlyList<ItemNamePart>>(new List<ItemNamePart> {new(item.DisplayName)})!,
        };

        BaseItem = item;
        DisplayName = displayName!;
        DisplayNameText = item.DisplayName;

        IsMarked = itemViewModelType is ItemViewModelType.Main
            ? parentTab.MarkedItems!.Map(m => m!.Any(i => i.Path == item.FullName?.Path))
            : new DeclarativeProperty<bool>(false);

        IsSelected = itemViewModelType is ItemViewModelType.Main
            ? parentTab.CurrentSelectedItem
                .Map(EqualsTo)
                .DistinctUntilChanged()
                .Debounce(TimeSpan.FromMilliseconds(1))
            : new DeclarativeProperty<bool>(IsInDeepestPath());

        IsAlternative = sourceCollection
            .Debounce(TimeSpan.FromMilliseconds(100))!
            .Map(c =>
                c?.Index().FirstOrDefault(i => EqualsTo(i.Value)).Key % 2 == 1
            );

        ViewMode = DeclarativePropertyHelpers
            .CombineLatest(IsMarked, IsSelected, IsAlternative, GenerateViewMode)
            .DistinctUntilChanged()
            .Debounce(TimeSpan.FromMilliseconds(1));
        Attributes = item.Attributes;
        CreatedAt = item.CreatedAt;
        ModifiedAt = item.ModifiedAt;
    }

    private Task<ItemViewMode> GenerateViewMode(bool isMarked, bool isSelected, bool isAlternative)
    {
        var result = (isMarked, isSelected, isAlternative) switch
        {
            (true, true, _) => ItemViewMode.MarkedSelected,
            (true, false, true) => ItemViewMode.MarkedAlternative,
            (false, true, _) => ItemViewMode.Selected,
            (false, false, true) => ItemViewMode.Alternative,
            (true, false, false) => ItemViewMode.Marked,
            _ => ItemViewMode.Default
        };

        return Task.FromResult(result);
    }


    public bool EqualsTo(IItemViewModel? itemViewModel) 
        => BaseItem?.FullName?.Path is { } path && path == itemViewModel?.BaseItem?.FullName?.Path;

    private bool IsInDeepestPath()
    {
        if (_parentTab?.Tab?.LastDeepestSelectedPath is null
            || BaseItem?.FullName is null)
        {
            return false;
        }

        var ownFullName = BaseItem.FullName;
        var deepestPath = _parentTab.Tab.LastDeepestSelectedPath;
        var commonPath = FullName.CreateSafe(PathHelper.GetCommonPath(ownFullName.Path, deepestPath.Path));

        return commonPath is not null && commonPath.Path == ownFullName.Path;
    }
}