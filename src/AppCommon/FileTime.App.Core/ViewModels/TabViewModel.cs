using System.Reactive.Linq;
using DynamicData;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.Core.Models;
using FileTime.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using MvvmGen;

namespace FileTime.App.Core.ViewModels;

[ViewModel]
public partial class TabViewModel : ITabViewModel, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IItemNameConverterService _itemNameConverterService;
    private readonly IAppState _appState;
    private readonly IRxSchedulerService _rxSchedulerService;
    private readonly SourceList<IAbsolutePath> _markedItems = new();
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;

    public ITab? Tab { get; private set; }
    public int TabNumber { get; private set; }

    public IObservable<bool> IsSelected { get; }

    public IObservable<IContainer?> CurrentLocation { get; private set; } = null!;
    public IObservable<IItemViewModel?> CurrentSelectedItem { get; private set; } = null!;
    public IObservable<IObservable<IChangeSet<IItemViewModel>>?> CurrentItems { get; private set; } = null!;
    public IObservable<IChangeSet<IAbsolutePath>> MarkedItems { get; }
    public IObservable<IObservable<IChangeSet<IItemViewModel>>?> SelectedsChildren { get; private set; } = null!;
    public IObservable<IObservable<IChangeSet<IItemViewModel>>?> ParentsChildren { get; private set; } = null!;

    public IObservable<IReadOnlyCollection<IItemViewModel>?> CurrentItemsCollectionObservable { get; private set; } =
        null!;

    public IObservable<IReadOnlyCollection<IItemViewModel>?> ParentsChildrenCollectionObservable { get; private set; } =
        null!;

    public IObservable<IReadOnlyCollection<IItemViewModel>?>
        SelectedsChildrenCollectionObservable { get; private set; } = null!;

    [Property] private BindedCollection<IItemViewModel>? _currentItemsCollection;

    [Property] private BindedCollection<IItemViewModel>? _parentsChildrenCollection;

    [Property] private BindedCollection<IItemViewModel>? _selectedsChildrenCollection;

    public TabViewModel(
        IServiceProvider serviceProvider,
        IItemNameConverterService itemNameConverterService,
        IAppState appState,
        IRxSchedulerService rxSchedulerService)
    {
        _serviceProvider = serviceProvider;
        _itemNameConverterService = itemNameConverterService;
        _appState = appState;

        MarkedItems = _markedItems.Connect().StartWithEmpty();
        IsSelected = _appState.SelectedTab.Select(s => s == this);
        _rxSchedulerService = rxSchedulerService;
    }

    public void Init(ITab tab, int tabNumber)
    {
        Tab = tab;
        TabNumber = tabNumber;

        CurrentLocation = tab.CurrentLocation.AsObservable();
        CurrentItems = tab.CurrentItems
            .Select(items => items?.Transform(i => MapItemToViewModel(i, ItemViewModelType.Main)))
            /*.ObserveOn(_rxSchedulerService.GetWorkerScheduler())
            .SubscribeOn(_rxSchedulerService.GetUIScheduler())*/
            .Publish(null)
            .RefCount();

        CurrentSelectedItem =
            Observable.CombineLatest(
                    CurrentItems,
                    tab.CurrentSelectedItem,
                    (currentItems, currentSelectedItemPath) =>
                        currentItems == null
                            ? Observable.Return((IItemViewModel?)null)
                            : currentItems
                                .ToCollection()
                                .Select(items =>
                                    items.FirstOrDefault(i => i.BaseItem?.FullName == currentSelectedItemPath?.Path))
                )
                .Switch()
                .Publish(null)
                .RefCount();

        SelectedsChildren = InitSelectedsChildren();
        ParentsChildren = InitParentsChildren();

        CurrentItemsCollectionObservable = InitAsd(CurrentItems);
        SelectedsChildrenCollectionObservable = InitAsd(SelectedsChildren);
        ParentsChildrenCollectionObservable = InitAsd(ParentsChildren);

        CurrentItemsCollection = new(CurrentItems);
        ParentsChildrenCollection = new(ParentsChildren);
        SelectedsChildrenCollection = new(SelectedsChildren);

        tab.CurrentLocation.Subscribe((_) => _markedItems.Clear());

        IObservable<IObservable<IChangeSet<IItemViewModel>>?> InitSelectedsChildren()
        {
            var currentSelectedItemThrottled =
                CurrentSelectedItem.Throttle(TimeSpan.FromMilliseconds(250)).Publish(null).RefCount();
            return Observable.Merge(
                    currentSelectedItemThrottled
                        .WhereNotNull()
                        .OfType<IContainerViewModel>()
                        .Where(c => c?.Container is not null)
                        .Select(c => c.Container!.Items)
                        .Switch()
                        .Select(i =>
                            i?.TransformAsync(MapItem)
                                .Transform(i => MapItemToViewModel(i, ItemViewModelType.SelectedChild))),
                    currentSelectedItemThrottled
                        .Where(c => c is null or not IContainerViewModel)
                        .Select(_ => (IObservable<IChangeSet<IItemViewModel>>?)null)
                )
                /*.ObserveOn(_rxSchedulerService.GetWorkerScheduler())
                .SubscribeOn(_rxSchedulerService.GetUIScheduler())*/
                .Publish(null)
                .RefCount();
        }

        IObservable<IObservable<IChangeSet<IItemViewModel>>?> InitParentsChildren()
        {
            var parentThrottled = CurrentLocation
                .Select(l => l?.Parent)
                .DistinctUntilChanged()
                .Publish(null)
                .RefCount();

            return Observable.Merge(
                    parentThrottled
                        .Where(p => p is not null)
                        .Select(p => Observable.FromAsync(async () => (IContainer)await p!.ResolveAsync()))
                        .Switch()
                        .Select(p => p.Items)
                        .Switch()
                        .Select(items =>
                            items?.TransformAsync(MapItem)
                                .Transform(i => MapItemToViewModel(i, ItemViewModelType.Parent))),
                    parentThrottled
                        .Where(p => p is null)
                        .Select(_ => (IObservable<IChangeSet<IItemViewModel>>?)null)
                )
                /*.ObserveOn(_rxSchedulerService.GetWorkerScheduler())
                .SubscribeOn(_rxSchedulerService.GetUIScheduler())*/
                .Publish(null)
                .RefCount();
        }

        IObservable<IReadOnlyCollection<IItemViewModel>?> InitAsd(
            IObservable<IObservable<IChangeSet<IItemViewModel>>?> source)
        {
            return source
                .Select(c =>
                    c != null ? c.ToCollection() : Observable.Return((IReadOnlyCollection<IItemViewModel>?)null))
                .Switch()
                .Publish(null)
                .RefCount();
        }
    }

    private static async Task<IItem> MapItem(IAbsolutePath item)
        => await item.ResolveAsync(forceResolve: true,
            itemInitializationSettings: new ItemInitializationSettings(true));

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
            var fileExtension = element.GetExtension<FileExtension>();

            if (fileExtension is not null)
            {
                var fileViewModel = _serviceProvider
                    .GetInitableResolver<IElement, FileExtension, ITabViewModel, ItemViewModelType>(
                        element, fileExtension, this, type)
                    .GetRequiredService<IFileViewModel>();

                return fileViewModel;
            }
            else
            {
                var elementViewModel = _serviceProvider
                    .GetInitableResolver<IElement, ITabViewModel, ItemViewModelType>(element, this, type)
                    .GetRequiredService<IElementViewModel>();

                return elementViewModel;
            }
        }

        throw new ArgumentException($"{nameof(item)} is not {nameof(IContainer)} neither {nameof(IElement)}");
    }

    public void AddMarkedItem(IAbsolutePath item) => _markedItems.Add(item);

    public void RemoveMarkedItem(IAbsolutePath item)
    {
        var itemsToRemove = _markedItems.Items.Where(i => i.Path.Path == item.Path.Path).ToList();

        _markedItems.RemoveMany(itemsToRemove);
    }

    public void ToggleMarkedItem(IAbsolutePath item)
    {
        if (_markedItems.Items.Any(i => i.Path.Path == item.Path.Path))
        {
            RemoveMarkedItem(item);
        }
        else
        {
            AddMarkedItem(item);
        }
    }

    public void ClearMarkedItems()
    {
        _markedItems.Clear();
    }

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