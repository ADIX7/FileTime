using System.ComponentModel;
using DeclarativeProperty;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Behaviors;
using FileTime.Core.Helper;
using FileTime.Core.Models;
using MoreLinq;
using PropertyChanged.SourceGenerator;

namespace FileTime.App.Core.ViewModels;

public abstract partial class ItemViewModel(IItemNameConverterService itemNameConverterService, IAppState appState) : IItemViewModel
{
    private ITabViewModel? _parentTab;

    [Notify] private IItem? _baseItem;

    [Notify] private string? _displayNameText;

    [Notify] private IDeclarativeProperty<bool> _isSelected = null!;

    [Notify] private IDeclarativeProperty<bool> _isMarked = null!;

    [Notify] private IDeclarativeProperty<ItemViewMode> _viewMode = null!;

    [Notify] private DateTime? _createdAt;
    [Notify] private DateTime? _modifiedAt;

    [Notify] private string? _attributes;

    [Notify] private IDeclarativeProperty<bool> _isAlternative = null!;

    public IDeclarativeProperty<IReadOnlyList<ItemNamePart>>? DisplayName { get; private set; }

    public void Init(
        IItem item,
        ITabViewModel parentTab,
        ItemViewModelType itemViewModelType)
    {
        _parentTab = parentTab;

        var sourceCollection = itemViewModelType switch
        {
            ItemViewModelType.Main => parentTab.CurrentItems,
            ItemViewModelType.Parent => parentTab.ParentsChildren,
            ItemViewModelType.SelectedChild => parentTab.SelectedsChildren,
            _ => throw new InvalidEnumArgumentException()
        };

        var displayName = itemViewModelType switch
        {
            ItemViewModelType.Main => appState.RapidTravelTextDebounced.Map(async s =>
                appState.ViewMode.Value != Models.Enums.ViewMode.RapidTravel
                && appState.SelectedTab.Value?.CurrentLocation.Value?.Provider is IItemNameConverterProvider nameConverterProvider
                    ? (IReadOnlyList<ItemNamePart>) await nameConverterProvider.GetItemNamePartsAsync(item)
                    : itemNameConverterService.GetDisplayName(item.DisplayName, s)
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