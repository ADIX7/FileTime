using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using FileTime.Core.Services;
using FileTime.Tools.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MvvmGen;

namespace FileTime.App.Core.ViewModels
{
    [ViewModel]
    public partial class TabViewModel : ITabViewModel, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IItemNameConverterService _itemNameConverterService;
        private readonly IAppState _appState;
        private readonly IRxSchedulerService _rxSchedulerService;
        private readonly BehaviorSubject<IEnumerable<FullName>> _markedItems = new(Enumerable.Empty<FullName>());
        private readonly List<FullName> _currentMarkedItems = new();
        private readonly List<IDisposable> _disposables = new();
        private bool disposed;

        public ITab? Tab { get; private set; }
        public int TabNumber { get; private set; }

        public IObservable<bool> IsSelected { get; }

        public IObservable<IContainer?> CurrentLocation { get; private set; } = null!;
        public IObservable<IItemViewModel?> CurrentSelectedItem { get; private set; } = null!;
        public IObservable<IObservable<IChangeSet<IItemViewModel>>?> CurrentItems { get; private set; } = null!;
        public IObservable<IEnumerable<FullName>> MarkedItems { get; }
        public IObservable<IObservable<IChangeSet<IItemViewModel>>?> SelectedsChildren { get; private set; } = null!;
        public IObservable<IObservable<IChangeSet<IItemViewModel>>?> ParentsChildren { get; private set; } = null!;

        public IObservable<IReadOnlyCollection<IItemViewModel>?> CurrentItemsCollectionObservable { get; private set; } = null!;

        [Property]
        private BindedCollection<IItemViewModel>? _currentItemsCollection;

        [Property]
        private BindedCollection<IItemViewModel>? _parentsChildrenCollection;

        [Property]
        private BindedCollection<IItemViewModel>? _selectedsChildrenCollection;

        public TabViewModel(
            IServiceProvider serviceProvider,
            IItemNameConverterService itemNameConverterService,
            IAppState appState,
            IRxSchedulerService rxSchedulerService)
        {
            _serviceProvider = serviceProvider;
            _itemNameConverterService = itemNameConverterService;
            _appState = appState;

            MarkedItems = _markedItems.Select(e => e.ToList()).AsObservable();
            IsSelected = _appState.SelectedTab.Select(s => s == this);
            _rxSchedulerService = rxSchedulerService;
        }

        public void Init(ITab tab, int tabNumber)
        {
            Tab = tab;
            TabNumber = tabNumber;

            CurrentLocation = tab.CurrentLocation.AsObservable();
            CurrentItems = tab.CurrentItems
                .Select(items => items?.Transform(MapItemToViewModel))
                .ObserveOn(_rxSchedulerService.GetWorkerScheduler())
                .SubscribeOn(_rxSchedulerService.GetUIScheduler())
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
                            .Select(items => items.FirstOrDefault(i => i.BaseItem?.FullName == currentSelectedItemPath?.Path))
                )
                .Switch()
                .Publish(null)
                .RefCount();

            SelectedsChildren = InitSelectedsChildren();
            ParentsChildren = InitParentsChildren();

            CurrentItemsCollectionObservable = CurrentItems
                .Select(c => c != null ? c.ToCollection() : Observable.Return((IReadOnlyCollection<IItemViewModel>?)null))
                .Switch()
                .Publish(null)
                .RefCount();

            CurrentItems.Subscribe(children =>
            {
                CurrentItemsCollection?.Dispose();
                CurrentItemsCollection = children.MapNull(c => new BindedCollection<IItemViewModel>(c!));
            });

            ParentsChildren.Subscribe(children =>
            {
                ParentsChildrenCollection?.Dispose();
                ParentsChildrenCollection = children.MapNull(c => new BindedCollection<IItemViewModel>(c!));
            });

            SelectedsChildren.Subscribe(children =>
            {
                SelectedsChildrenCollection?.Dispose();
                SelectedsChildrenCollection = children.MapNull(c => new BindedCollection<IItemViewModel>(c!));
            });

            tab.CurrentLocation.Subscribe((_) => _markedItems.OnNext(Enumerable.Empty<FullName>()));

            IObservable<IObservable<IChangeSet<IItemViewModel>>?> InitSelectedsChildren()
            {
                var currentSelectedItemThrottled = CurrentSelectedItem.Throttle(TimeSpan.FromMilliseconds(250)).Publish(null).RefCount();
                return Observable.Merge(
                    currentSelectedItemThrottled
                        .WhereNotNull()
                        .OfType<IContainerViewModel>()
                        .Where(c => c?.Container is not null)
                        .Select(c => c.Container!.Items)
                        .Switch()
                        .Select(i => i?.TransformAsync(MapItem).Transform(MapItemToViewModel)),
                    currentSelectedItemThrottled
                        .Where(c => c is null || c is not IContainerViewModel)
                        .Select(_ => (IObservable<IChangeSet<IItemViewModel>>?)null)
                )
                .ObserveOn(_rxSchedulerService.GetWorkerScheduler())
                .SubscribeOn(_rxSchedulerService.GetUIScheduler())
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
                        .Select(items => items?.TransformAsync(MapItem).Transform(MapItemToViewModel)),
                    parentThrottled
                        .Where(p => p is null)
                        .Select(_ => (IObservable<IChangeSet<IItemViewModel>>?)null)
                )
                .ObserveOn(_rxSchedulerService.GetWorkerScheduler())
                .SubscribeOn(_rxSchedulerService.GetUIScheduler())
                .Publish(null)
                .RefCount();
            }
        }

        private static async Task<IItem> MapItem(IAbsolutePath item)
            => await item.ResolveAsync(forceResolve: true, itemInitializationSettings: new ItemInitializationSettings(true));

        private IItemViewModel MapItemToViewModel(IItem item)
        {
            if (item is IContainer container)
            {
                var containerViewModel = _serviceProvider.GetInitableResolver<IContainer, ITabViewModel>(container, this).GetRequiredService<IContainerViewModel>();

                return containerViewModel;
            }
            else if (item is IFileElement fileElement)
            {
                var fileViewModel = _serviceProvider.GetInitableResolver<IFileElement, ITabViewModel>(fileElement, this).GetRequiredService<IFileViewModel>();
                fileViewModel.Size = fileElement.Size;

                return fileViewModel;
            }
            else if (item is IElement element)
            {
                var elementViewModel = _serviceProvider.GetInitableResolver<IElement, ITabViewModel>(element, this).GetRequiredService<IElementViewModel>();

                return elementViewModel;
            }

            throw new ArgumentException($"{nameof(item)} is not {nameof(IContainer)} neither {nameof(IElement)}");
        }

        public void AddMarkedItem(FullName item)
        {
            _currentMarkedItems.Add(item);
            _markedItems.OnNext(_currentMarkedItems);
        }

        public void RemoveMarkedItem(FullName item)
        {
            _currentMarkedItems.RemoveAll(i => i.Path == item.Path);
            _markedItems.OnNext(_currentMarkedItems);
        }

        public void ToggleMarkedItem(FullName item)
        {
            if (_currentMarkedItems.Any(i => i.Path == item.Path))
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
            _currentMarkedItems.Clear();
            _markedItems.OnNext(_currentMarkedItems);
        }

        ~TabViewModel() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                foreach (var disposable in _disposables)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch { }
                }
            }
            disposed = true;
        }
    }
}