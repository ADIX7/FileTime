using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using FileTime.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Core.ViewModels
{
    public class TabViewModel : ITabViewModel, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IItemNameConverterService _itemNameConverterService;
        private readonly IAppState _appState;
        private readonly IRxSchedulerService _rxSchedulerService;
        private readonly BehaviorSubject<IEnumerable<FullName>> _markedItems = new(Enumerable.Empty<FullName>());
        private readonly List<IDisposable> _disposables = new();
        private bool disposed;

        public ITab? Tab { get; private set; }
        public int TabNumber { get; private set; }

        public IObservable<bool> IsSelected { get; }

        public IObservable<IContainer?> CurrentLocation { get; private set; } = null!;
        public IObservable<IItemViewModel?> CurrentSelectedItem { get; private set; } = null!;
        public IObservable<IReadOnlyList<IItemViewModel>> CurrentItems { get; private set; } = null!;
        public IObservable<IEnumerable<FullName>> MarkedItems { get; }
        public IObservable<IReadOnlyList<IItemViewModel>?> SelectedsChildren { get; private set; } = null!;
        public IObservable<IReadOnlyList<IItemViewModel>?> ParentsChildren { get; private set; } = null!;

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
                .Select(items =>
                    items == null
                    ? new List<IItemViewModel>()
                    : items.Select(MapItemToViewModel).ToList())
                .ObserveOn(_rxSchedulerService.GetWorkerScheduler())
                .SubscribeOn(_rxSchedulerService.GetUIScheduler())
                .Publish(new List<IItemViewModel>())
                .RefCount();

            CurrentSelectedItem =
                Observable.CombineLatest(
                    CurrentItems,
                    tab.CurrentSelectedItem,
                    (currentItems, currentSelectedItemPath) => currentItems.FirstOrDefault(i => i.BaseItem?.FullName == currentSelectedItemPath?.Path)
                )
                .Publish(null)
                .RefCount();

            var currentSelectedItemThrottled = CurrentSelectedItem.Throttle(TimeSpan.FromMilliseconds(250)).Publish(null).RefCount();
            SelectedsChildren = Observable.Merge(
                currentSelectedItemThrottled
                    .WhereNotNull()
                    .OfType<IContainerViewModel>()
                    .Where(c => c?.Container is not null)
                    .Select(c => c.Container!.Items)
                    .Switch()
                    .Select(items => Observable.FromAsync(async () => await Map(items)))
                    .Switch()
                    .Select(items => items?.Select(MapItemToViewModel).ToList()),
                currentSelectedItemThrottled
                    .Where(c => c is null || c is not IContainerViewModel)
                    .Select(_ => (IReadOnlyList<IItemViewModel>?)null)
            )
            .ObserveOn(_rxSchedulerService.GetWorkerScheduler())
            .SubscribeOn(_rxSchedulerService.GetUIScheduler())
            .Publish(null)
            .RefCount();

            var parentThrottled = CurrentLocation
                .Select(l => l?.Parent)
                .DistinctUntilChanged()
                .Publish(null)
                .RefCount();

            ParentsChildren = Observable.Merge(
                parentThrottled
                    .Where(p => p is not null)
                    .Select(p => Observable.FromAsync(async () => (IContainer)await p!.ResolveAsync()))
                    .Switch()
                    .Select(p => p.Items)
                    .Switch()
                    .Select(items => Observable.FromAsync(async () => await Map(items)))
                    .Switch()
                    .Select(items => items?.Select(MapItemToViewModel).ToList()),
                parentThrottled
                    .Where(p => p is null)
                    .Select(_ => (IReadOnlyList<IItemViewModel>?)null)
            )
            .ObserveOn(_rxSchedulerService.GetWorkerScheduler())
            .SubscribeOn(_rxSchedulerService.GetUIScheduler())
            .Publish(null)
            .RefCount();

            tab.CurrentLocation.Subscribe((_) => _markedItems.OnNext(Enumerable.Empty<FullName>()));

            static async Task<List<IItem>?> Map(IEnumerable<IAbsolutePath>? items)
            {
                if (items == null) return null;

                return await items
                    .ToAsyncEnumerable()
                    .SelectAwait(async i => await i.ResolveAsync(forceResolve: true, itemInitializationSettings: new ItemInitializationSettings(true)))
                    .ToListAsync();
            }
        }

        private IItemViewModel MapItemToViewModel(IItem item, int index)
        {
            if (item is IContainer container)
            {
                var containerViewModel = _serviceProvider.GetInitableResolver<IContainer, ITabViewModel, int>(container, this, index).GetRequiredService<IContainerViewModel>();

                return containerViewModel;
            }
            else if (item is IFileElement fileElement)
            {
                var fileViewModel = _serviceProvider.GetInitableResolver<IFileElement, ITabViewModel, int>(fileElement, this, index).GetRequiredService<IFileViewModel>();
                fileViewModel.Size = fileElement.Size;

                return fileViewModel;
            }
            else if (item is IElement element)
            {
                var elementViewModel = _serviceProvider.GetInitableResolver<IElement, ITabViewModel, int>(element, this, index).GetRequiredService<IElementViewModel>();

                return elementViewModel;
            }

            throw new ArgumentException($"{nameof(item)} is not {nameof(IContainer)} neighter {nameof(IElement)}");
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