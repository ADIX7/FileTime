using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using DeclarativeProperty;
using DynamicData;
using DynamicData.Binding;
using FileTime.App.Core.Services;
using FileTime.Core.Helper;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using ObservableComputations;

namespace FileTime.Core.Services;

public class Tab : ITab
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ITabEvents _tabEvents;
    private readonly DeclarativeProperty<IContainer?> _currentLocation = new(null);
    private readonly BehaviorSubject<IContainer?> _currentLocationForced = new(null);
    private readonly DeclarativeProperty<AbsolutePath?> _currentRequestItem = new(null);
    private readonly ObservableCollection<ItemFilter> _itemFilters = new();
    private readonly DeclarativeProperty<ObservableCollection<ItemFilter>?> _itemFiltersProperty;
    private AbsolutePath? _currentSelectedItemCached;
    private PointInTime _currentPointInTime;
    private CancellationTokenSource? _setCurrentLocationCancellationTokenSource;
    private CancellationTokenSource? _setCurrentItemCancellationTokenSource;

    public IDeclarativeProperty<IContainer?> CurrentLocation { get; }
    public IDeclarativeProperty<ObservableCollection<IItem>?> CurrentItems { get; }
    public IDeclarativeProperty<AbsolutePath?> CurrentSelectedItem { get; }
    public FullName? LastDeepestSelectedPath { get; private set; }

    public Tab(
        ITimelessContentProvider timelessContentProvider,
        ITabEvents tabEvents,
        IRefreshSmoothnessCalculator refreshSmoothnessCalculator)
    {
        _timelessContentProvider = timelessContentProvider;
        _tabEvents = tabEvents;
        _currentPointInTime = null!;
        _itemFiltersProperty = new(_itemFilters);

        _timelessContentProvider.CurrentPointInTime.Subscribe(p => _currentPointInTime = p);

        CurrentLocation = _currentLocation;
        CurrentLocation.Subscribe((c, _) =>
        {
            if (_currentSelectedItemCached is not null)
            {
                LastDeepestSelectedPath = FullName.CreateSafe(PathHelper.GetLongerPath(LastDeepestSelectedPath?.Path, _currentSelectedItemCached.Path.Path));
            }

            return Task.CompletedTask;
        });

        CurrentItems = DeclarativePropertyHelpers.CombineLatest(
            CurrentLocation,
            _itemFiltersProperty.Watch<ObservableCollection<ItemFilter>, ItemFilter>(),
            (container, filters) =>
            {
                ObservableCollection<IItem>? items = null;

                if (container is not null)
                {
                    items = container
                        .Items
                        .Selecting<AbsolutePath, IItem>(i => MapItem(i));

                    if (filters is not null)
                    {
                        items = items.Filtering(i => filters.All(f => f.Filter(i)));
                    }
                }

                return Task.FromResult(items);
            }
        );


        CurrentSelectedItem = DeclarativePropertyHelpers.CombineLatest(
            CurrentItems.Watch<ObservableCollection<IItem>, IItem>(),
            _currentRequestItem.DistinctUntilChanged(),
            (items, selected) =>
            {
                if (selected != null && (items?.Any(i => i.FullName == selected.Path) ?? true)) return Task.FromResult<AbsolutePath?>(selected);
                if (items == null || items.Count == 0) return Task.FromResult<AbsolutePath?>(null);

                return Task.FromResult(GetSelectedItemByItems(items));
            }).DistinctUntilChanged();

        CurrentSelectedItem.Subscribe((v) =>
        {
            refreshSmoothnessCalculator.RegisterChange();
            refreshSmoothnessCalculator.RecalculateSmoothness();
        });

        CurrentSelectedItem.Subscribe(async (s, _) =>
        {
            _currentSelectedItemCached = s;
            await _currentRequestItem.SetValue(s);
        });
    }

    static void UpdateConsumer<T>(ObservableCollection<T>? collection, ref OcConsumer? consumer)
    {
        if (collection is not IComputing computing) return;

        consumer?.Dispose();
        consumer = new OcConsumer();
        computing.For(consumer);
    }

    private static IItem MapItem(AbsolutePath item)
    {
        var t = Task.Run(async () => await item.ResolveAsync(true));
        t.Wait();
        return t.Result;
    }

    private static SortExpressionComparer<IItem> SortItems()
        //TODO: Order
        => SortExpressionComparer<IItem>
            .Ascending(i => i.Type)
            .ThenByAscending(i => i.DisplayName.ToLower());


    public async Task InitAsync(IContainer currentLocation)
        => await _currentLocation.SetValue(currentLocation);

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
                    .Skip(parentPath == "" ? 0 : parentPath.Split(Constants.SeparatorChar).Length)
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

    public async Task SetCurrentLocation(IContainer newLocation)
    {
        _setCurrentLocationCancellationTokenSource?.Cancel();
        _setCurrentLocationCancellationTokenSource = new CancellationTokenSource();
        await _currentLocation.SetValue(newLocation, _setCurrentLocationCancellationTokenSource.Token);

        if (newLocation.FullName != null)
        {
            _tabEvents.OnLocationChanged(this, newLocation.FullName);
        }
    }

    public async Task ForceSetCurrentLocation(IContainer newLocation)
    {
        _setCurrentLocationCancellationTokenSource?.Cancel();
        _setCurrentLocationCancellationTokenSource = new CancellationTokenSource();
        await _currentLocation.SetValue(newLocation, _setCurrentLocationCancellationTokenSource.Token);

        if (newLocation.FullName != null)
        {
            _tabEvents.OnLocationChanged(this, newLocation.FullName);
        }
    }

    public async Task SetSelectedItem(AbsolutePath newSelectedItem)
    {
        _setCurrentItemCancellationTokenSource?.Cancel();
        _setCurrentItemCancellationTokenSource = new CancellationTokenSource();
        await _currentRequestItem.SetValue(newSelectedItem, _setCurrentItemCancellationTokenSource.Token);
    }

    public void AddItemFilter(ItemFilter filter) => _itemFilters.Add(filter);
    public void RemoveItemFilter(ItemFilter filter) => _itemFilters.Remove(filter);

    public void RemoveItemFilter(string name)
    {
        var itemsToRemove = _itemFilters.Where(t => t.Name == name).ToList();
        _itemFilters.RemoveMany(itemsToRemove);
    }

    public async Task OpenSelected()
    {
        if (_currentSelectedItemCached == null) return;
        var resolvedSelectedItem =
            await _currentSelectedItemCached.TimelessProvider.GetItemByFullNameAsync(_currentSelectedItemCached.Path, _currentPointInTime);

        if (resolvedSelectedItem is not IContainer resolvedContainer) return;
        await SetCurrentLocation(resolvedContainer);
    }

    public void Dispose()
        => _currentLocation.Dispose();
}