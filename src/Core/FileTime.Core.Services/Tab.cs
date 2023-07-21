using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using DeclarativeProperty;
using DynamicData;
using DynamicData.Binding;
using FileTime.App.Core.Services;
using FileTime.Core.Helper;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using ObservableComputations;
using static System.DeferTools;

namespace FileTime.Core.Services;

public class Tab : ITab
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ITabEvents _tabEvents;
    private readonly IRefreshSmoothnessCalculator _refreshSmoothnessCalculator;
    private readonly DeclarativeProperty<IContainer?> _currentLocation = new(null);
    private readonly BehaviorSubject<IContainer?> _currentLocationForced = new(null);
    private readonly DeclarativeProperty<AbsolutePath?> _currentRequestItem = new(null);
    private readonly SourceList<ItemFilter> _itemFilters = new();
    private AbsolutePath? _currentSelectedItemCached;
    private PointInTime _currentPointInTime;
    private OcConsumer? _currentItemsConsumer;
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
        _refreshSmoothnessCalculator = refreshSmoothnessCalculator;
        _currentPointInTime = null!;

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

        /*CurrentLocation = _currentLocation
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
            .RefCount();*/

        CurrentItems = CurrentLocation.Map((container, _) =>
            {
                var items = container is null
                    ? (ObservableCollection<IItem>?) null
                    : container.Items.Selecting<AbsolutePath, IItem>(i => MapItem(i));
                return Task.FromResult(items);
            }
        ) /*.Watch<ObservableCollection<IItem>, IItem>()*/;
        /*using var _ = Defer(() =>
            CurrentItems.Subscribe(c => UpdateConsumer(c, ref _currentItemsConsumer))
        );*/

        /*CurrentItems.RegisterTrigger(
            (sender, items) =>
            {
                if (items is null)
                    return null;

                items.CollectionChanged += Handler;

                return Disposable.Create(() => items.CollectionChanged -= Handler);

                void Handler(object? o, NotifyCollectionChangedEventArgs e)
                {
                    var t = Task.Run(async () => await sender.ReFireAsync());
                    t.Wait();
                }
            });*/

        /*CurrentItems =
            Observable.Merge(
                    Observable.CombineLatest(
                        CurrentLocation
                            .Where(c => c is not null)
                            .Select(c => c!.ItemsCollection)
                            .Select(items => items.TransformAsync(MapItem)),
                        _itemFilters.Connect().StartWithEmpty().ToCollection(),
                        (items, filters) =>
                            //Note: Dont user Sort before where, as DynamicData cant handle
                            //sort in (so that's if they are before) filters
                            items
                                .Where(i => filters.All(f => f.Filter(i)))
                                .Sort(SortItems())
                    ),
                    CurrentLocation
                        .Where(c => c is null)
                        .Select(_ => (IObservable<IChangeSet<IItem, string>>?) null)
                )
                .Publish(null)
                .RefCount();*/
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
            _refreshSmoothnessCalculator.RegisterChange();
            _refreshSmoothnessCalculator.RecalculateSmoothness();
        });


        /*CurrentSelectedItem =
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
                .RefCount();*/

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
        var itemsToRemove = _itemFilters.Items.Where(t => t.Name == name).ToList();
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
    {
        _currentLocation.Dispose();
        _itemFilters.Dispose();
    }
}