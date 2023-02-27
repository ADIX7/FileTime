using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using DynamicData.Alias;
using DynamicData.Binding;
using FileTime.Core.Helper;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Services;

public class Tab : ITab
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ITabEvents _tabEvents;
    private readonly BehaviorSubject<IContainer?> _currentLocation = new(null);
    private readonly BehaviorSubject<IContainer?> _currentLocationForced = new(null);
    private readonly BehaviorSubject<AbsolutePath?> _currentSelectedItem = new(null);
    private readonly SourceList<ItemFilter> _itemFilters = new();
    private AbsolutePath? _currentSelectedItemCached;
    private PointInTime _currentPointInTime;

    public IObservable<IContainer?> CurrentLocation { get; }
    public IObservable<IObservable<IChangeSet<IItem, string>>?> CurrentItems { get; }
    public IObservable<AbsolutePath?> CurrentSelectedItem { get; }
    public FullName? LastDeepestSelectedPath { get; private set; }

    public Tab(ITimelessContentProvider timelessContentProvider, ITabEvents tabEvents)
    {
        _timelessContentProvider = timelessContentProvider;
        _tabEvents = tabEvents;
        _currentPointInTime = null!;

        _timelessContentProvider.CurrentPointInTime.Subscribe(p => _currentPointInTime = p);

        CurrentLocation = _currentLocation
            .DistinctUntilChanged()
            .Merge(_currentLocationForced)
            .Do(_ =>
            {
                if (_currentSelectedItemCached is not null)
                {
                    LastDeepestSelectedPath = FullName.CreateSafe(PathHelper.GetLongerPath(LastDeepestSelectedPath?.Path, _currentSelectedItemCached.Path.Path));
                }
            })
            .Publish(null)
            .RefCount();

        CurrentItems =
            Observable.Merge(
                    Observable.CombineLatest(
                        CurrentLocation
                            .Where(c => c is not null)
                            .Select(c => c!.Items)
                            .Select(items => items.TransformAsync(MapItem)),
                        _itemFilters.Connect().StartWithEmpty().ToCollection(),
                        (items, filters) =>
                            items
                                ?.Where(i => filters.All(f => f.Filter(i)))
                                .Sort(SortItems())
                    ),
                    CurrentLocation
                        .Where(c => c is null)
                        .Select(_ => (IObservable<IChangeSet<IItem, string>>?) null)
                )
                .Publish(null)
                .RefCount();

        CurrentSelectedItem =
            Observable.CombineLatest(
                    CurrentItems
                        .Select(c =>
                            c == null
                                ? Observable.Return<IReadOnlyCollection<IItem>?>(null)
                                : c.ToCollection()
                        )
                        .Switch(),
                    _currentSelectedItem,
                    (items, selected) =>
                    {
                        if (selected != null && (items?.Any(i => i.FullName == selected.Path) ?? true)) return selected;
                        if (items == null || items.Count == 0) return null;

                        return GetSelectedItemByItems(items);
                    }
                )
                .DistinctUntilChanged()
                .Publish(null)
                .RefCount();

        CurrentSelectedItem.Subscribe(s =>
        {
            _currentSelectedItemCached = s;
            _currentSelectedItem.OnNext(s);
        });
    }

    private static SortExpressionComparer<IItem> SortItems()
        //TODO: Order
        => SortExpressionComparer<IItem>
            .Ascending(i => i.Type)
            .ThenByAscending(i => i.DisplayName?.ToLower() ?? "");

    private async Task<IItem> MapItem(AbsolutePath item) => await item.ResolveAsync(true);

    public void Init(IContainer currentLocation)
    {
        _currentLocation.OnNext(currentLocation);
    }

    private AbsolutePath? GetSelectedItemByItems(IReadOnlyCollection<IItem> items)
    {
        if (!items.Any()) return null;

        var newSelectedItem = new AbsolutePath(_timelessContentProvider, items.First());
        if (LastDeepestSelectedPath is not null)
        {
            var parentPath = items.First().FullName?.GetParent()?.Path;

            if (parentPath is not null && LastDeepestSelectedPath.Path.StartsWith(parentPath))
            {
                var itemNameToSelect = LastDeepestSelectedPath.Path
                    .Split(Constants.SeparatorChar)
                    .Skip(parentPath.Split(Constants.SeparatorChar).Length)
                    .FirstOrDefault();

                var itemToSelect = items.FirstOrDefault(i => i.FullName?.GetName() == itemNameToSelect);

                if (itemToSelect != null)
                {
                    newSelectedItem = new AbsolutePath(_timelessContentProvider, itemToSelect);
                }
            }
        }

        LastDeepestSelectedPath = FullName.CreateSafe(PathHelper.GetLongerPath(LastDeepestSelectedPath?.Path, newSelectedItem.Path.Path));

        return newSelectedItem;
    }

    public void SetCurrentLocation(IContainer newLocation)
    {
        _currentLocation.OnNext(newLocation);

        if (newLocation.FullName != null)
        {
            _tabEvents.OnLocationChanged(this, newLocation.FullName);
        }
    }

    public void ForceSetCurrentLocation(IContainer newLocation)
    {
        _currentLocationForced.OnNext(newLocation);

        if (newLocation.FullName != null)
        {
            _tabEvents.OnLocationChanged(this, newLocation.FullName);
        }
    }

    public void SetSelectedItem(AbsolutePath newSelectedItem) => _currentSelectedItem.OnNext(newSelectedItem);

    public void AddItemFilter(ItemFilter filter) => _itemFilters.Add(filter);
    public void RemoveItemFilter(ItemFilter filter) => _itemFilters.Remove(filter);

    public void RemoveItemFilter(string name)
    {
        var itemsToRemove = _itemFilters.Items.Where(t => t.Name == name).ToList();
        _itemFilters.RemoveMany(itemsToRemove);
    }

    public async Task OpenSelected()
    {
        if (_currentSelectedItemCached == null) return;
        var resolvedSelectedItem =
            await _currentSelectedItemCached.TimelessProvider.GetItemByFullNameAsync(_currentSelectedItemCached.Path, _currentPointInTime);

        if (resolvedSelectedItem is not IContainer resolvedContainer) return;
        SetCurrentLocation(resolvedContainer);
    }

    public void Dispose()
    {
        _currentLocation.Dispose();
        _currentSelectedItem.Dispose();
        _itemFilters.Dispose();
    }
}