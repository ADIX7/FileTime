using System.Collections.ObjectModel;
using CircularBuffer;
using DeclarativeProperty;
using DynamicData;
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
    private readonly DeclarativeProperty<IContainer?> _currentLocationForced = new(null);
    private readonly DeclarativeProperty<AbsolutePath?> _currentRequestItem = new(null);
    private readonly ObservableCollection<ItemFilter> _itemFilters = new();
    private readonly CircularBuffer<FullName> _history = new(20);
    private readonly CircularBuffer<FullName> _future = new(20);
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
        var itemFiltersProperty = new DeclarativeProperty<ObservableCollection<ItemFilter>>(_itemFilters)
            .Watch<ObservableCollection<ItemFilter>, ItemFilter>();

        _timelessContentProvider.CurrentPointInTime.Subscribe(p => _currentPointInTime = p);

        CurrentLocation = DeclarativePropertyHelpers.Merge(
            _currentLocation.DistinctUntilChanged(),
            _currentLocationForced
        );

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
            itemFiltersProperty,
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

    private static IItem MapItem(AbsolutePath item)
    {
        var t = Task.Run(async () => await item.ResolveAsync(true));
        t.Wait();
        return t.Result;
    }


    public async Task InitAsync(IContainer currentLocation)
        => await SetCurrentLocation(currentLocation);

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
        _future.Clear();
        await SetCurrentLocation(newLocation, true);
    }

    private async Task SetCurrentLocation(IContainer newLocation, bool addToHistory)
    {
        _setCurrentLocationCancellationTokenSource?.Cancel();
        _setCurrentLocationCancellationTokenSource = new CancellationTokenSource();

        if (addToHistory
            && newLocation.FullName is { } fullName
            && (_history.Count == 0
                || _history.Last() != fullName))
        {
            _history.PushFront(fullName);
        }

        await _currentLocation.SetValue(newLocation, _setCurrentLocationCancellationTokenSource.Token);

        if (newLocation.FullName != null)
        {
            _tabEvents.OnLocationChanged(this, newLocation);
        }
    }

    public async Task ForceSetCurrentLocation(IContainer newLocation)
    {
        _setCurrentLocationCancellationTokenSource?.Cancel();
        _setCurrentLocationCancellationTokenSource = new CancellationTokenSource();
        await _currentLocationForced.SetValue(newLocation, _setCurrentLocationCancellationTokenSource.Token);

        if (newLocation.FullName != null)
        {
            _tabEvents.OnLocationChanged(this, newLocation);
        }
    }

    public async Task GoBackAsync()
    {
        if (_history.Count < 2) return;

        var currentLocationFullName = _history.PopFront();
        _future.PushFront(currentLocationFullName);

        var lastLocationFullName = _history.First();
        var container = (IContainer) await _timelessContentProvider.GetItemByFullNameAsync(
            lastLocationFullName,
            PointInTime.Present);
        await SetCurrentLocation(container, false);
    }

    public async Task GoForwardAsync()
    {
        if (_future.Count == 0) return;

        var fullName = _future.PopFront();
        _history.PushFront(fullName);
        var container = (IContainer) await _timelessContentProvider.GetItemByFullNameAsync(
            fullName,
            PointInTime.Present);
        await SetCurrentLocation(container, false);
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