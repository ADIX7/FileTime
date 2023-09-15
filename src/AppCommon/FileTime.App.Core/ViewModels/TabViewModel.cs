using System.Collections.ObjectModel;
using DeclarativeProperty;
using DynamicData;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using FileTime.Core.Services;
using FileTime.Core.Timeline;
using InitableService;
using ObservableComputations;
using IContainer = FileTime.Core.Models.IContainer;
using static System.DeferTools;

namespace FileTime.App.Core.ViewModels;

public class TabViewModel : ITabViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IRefreshSmoothnessCalculator _refreshSmoothnessCalculator;
    private readonly ObservableCollection<FullName> _markedItems = new();
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;
    private OcConsumer? _currentItemsConsumer;
    private OcConsumer? _selectedsChildrenConsumer;
    private OcConsumer? _parentsChildrenConsumer;

    public ITab? Tab { get; private set; }
    public int TabNumber { get; private set; }

    public IDeclarativeProperty<bool> IsSelected { get; } = null!;

    public IDeclarativeProperty<IContainer?> CurrentLocation { get; private set; } = null!;
    public IDeclarativeProperty<IItemViewModel?> CurrentSelectedItem { get; private set; } = null!;
    public IDeclarativeProperty<int?> CurrentSelectedItemIndex { get; set; } = null!;
    public IDeclarativeProperty<IContainerViewModel?> CurrentSelectedItemAsContainer { get; private set; } = null!;
    public IDeclarativeProperty<ObservableCollection<IItemViewModel>?> CurrentItems { get; private set; } = null!;
    public IDeclarativeProperty<ObservableCollection<FullName>> MarkedItems { get; } = null!;
    public IDeclarativeProperty<ObservableCollection<IItemViewModel>?> SelectedsChildren { get; private set; } = null!;
    public IDeclarativeProperty<ObservableCollection<IItemViewModel>?> ParentsChildren { get; private set; } = null!;


    public TabViewModel(
        IServiceProvider serviceProvider,
        IAppState appState,
        ITimelessContentProvider timelessContentProvider,
        IRefreshSmoothnessCalculator refreshSmoothnessCalculator)
    {
        _serviceProvider = serviceProvider;

        MarkedItems = _markedItems.Watch()!;
        IsSelected = appState.SelectedTab.Map(s => s == this);
        _timelessContentProvider = timelessContentProvider;
        _refreshSmoothnessCalculator = refreshSmoothnessCalculator;
    }

    public void Init(ITab tab, int tabNumber)
    {
        Tab = tab;
        TabNumber = tabNumber;

        tab.AddToDisposables(_disposables);

        CurrentLocation = tab.CurrentLocation;

        CurrentItems =
            tab.CurrentItems
                .Map(items =>
                    Task.FromResult<ObservableCollection<IItemViewModel>?>(
                        items?.Selecting<IItem, IItemViewModel>(
                            i => MapItemToViewModel(i, ItemViewModelType.Main)
                        )
                    )
                )!;

        using var _ = Defer(
            () => CurrentItems.Subscribe(c => UpdateConsumer(c, ref _currentItemsConsumer))
        );

        CurrentSelectedItem = DeclarativePropertyHelpers.CombineLatest(
            tab.CurrentSelectedItem,
            CurrentItems.Watch<ObservableCollection<IItemViewModel>, IItemViewModel>(),
            (currentSelectedItem, currentItems) =>
                Task.FromResult(currentItems?.FirstOrDefault(i => i.BaseItem?.FullName?.Path == currentSelectedItem?.Path.Path)),
            item =>
            {
                if (item?.BaseItem is not { } baseItem)
                    return;

                tab.SetSelectedItem(new AbsolutePath(_timelessContentProvider, baseItem));
            }
        );

        CurrentSelectedItemIndex = DeclarativePropertyHelpers.CombineLatest(
            tab.CurrentSelectedItem,
            CurrentItems.Watch<ObservableCollection<IItemViewModel>, IItemViewModel>(),
            (currentSelectedItem, currentItems) =>
            {
                if (currentItems is null || currentSelectedItem is null)
                    return Task.FromResult<int?>(-1);

                for (var i = 0; i < currentItems.Count; i++)
                {
                    if (currentItems[i].BaseItem?.FullName?.Path == currentSelectedItem?.Path.Path)
                    {
                        return Task.FromResult<int?>(i);
                    }
                }

                return Task.FromResult<int?>(-1);
            });

        CurrentSelectedItem.Subscribe(_ =>
        {
            _refreshSmoothnessCalculator.RegisterChange();
            _refreshSmoothnessCalculator.RecalculateSmoothness();
        });

        CurrentSelectedItemAsContainer = CurrentSelectedItem.Map(i => i as IContainerViewModel);

        SelectedsChildren = CurrentSelectedItem
            .Debounce(_ => _refreshSmoothnessCalculator.RefreshDelay, resetTimer: true)
            .DistinctUntilChanged()
            .Map(item =>
            {
                if (item is not IContainerViewModel {Container: { } container})
                    return (ObservableCollection<IItemViewModel>?) null;

                var items = container
                    .Items
                    .Selecting(i => MapItem(i))
                    .Ordering(i => i.Type)
                    .ThenOrdering(i => i.Name)
                    .Selecting(i => MapItemToViewModel(i, ItemViewModelType.SelectedChild));

                return items;
            });
        using var __ = Defer(() =>
            SelectedsChildren.Subscribe(c => UpdateConsumer(c, ref _selectedsChildrenConsumer))
        );

        ParentsChildren = CurrentLocation.Map(async item =>
        {
            if (item?.Parent is null) return (ObservableCollection<IItemViewModel>?) null;
            var parent = (IContainer) await item.Parent.ResolveAsync();

            var items = parent.Items
                .Selecting(i => MapItem(i))
                .Ordering(i => i.Type)
                .ThenOrdering(i => i.Name)
                .Selecting(i => MapItemToViewModel(i, ItemViewModelType.Parent));

            return items;
        })!;
        using var ___ = Defer(() =>
            ParentsChildren.Subscribe(c => UpdateConsumer(c, ref _parentsChildrenConsumer))
        );

        tab.CurrentLocation.Subscribe(_ => _markedItems.Clear()).AddToDisposables(_disposables);
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
        var t = Task.Run(async () => 
            await MapItemAsync(item) 
            ?? throw new Exception("Could not resolve path " + item.Path.Path));
        t.Wait();
        return t.Result;
    }

    private static async Task<IItem> MapItemAsync(AbsolutePath item)
        => await item.ResolveAsync(forceResolve: true,
            itemInitializationSettings: new ItemInitializationSettings {SkipChildInitialization = true});

    private IItemViewModel MapItemToViewModel(IItem item, ItemViewModelType type)
    {
        if (item is IContainer container)
        {
            var containerViewModel = _serviceProvider
                .GetInitableResolver<IContainer, ITabViewModel, ItemViewModelType>(container, this, type)
                .GetRequiredService<IContainerViewModel>();

            return containerViewModel;
        }
        else if (item is IElement element)
        {
            var elementViewModel = _serviceProvider
                .GetInitableResolver<IElement, ITabViewModel, ItemViewModelType>(element, this, type)
                .GetRequiredService<IElementViewModel>();

            return elementViewModel;
        }

        throw new ArgumentException($"{nameof(item)} is not {nameof(IContainer)} neither {nameof(IElement)}");
    }

    public void AddMarkedItem(FullName fullName) => _markedItems.Add(fullName);

    public void RemoveMarkedItem(FullName fullName)
    {
        var itemsToRemove = _markedItems.Where(i => i.Path == fullName.Path).ToList();

        _markedItems.RemoveMany(itemsToRemove);
    }

    public void ToggleMarkedItem(FullName fullName)
    {
        if (_markedItems.Any(i => i.Path == fullName.Path))
        {
            RemoveMarkedItem(fullName);
        }
        else
        {
            AddMarkedItem(fullName);
        }
    }

    public void ClearMarkedItems()
        => _markedItems.Clear();

    ~TabViewModel() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                }
            }
        }

        _disposed = true;
    }
}