using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.App.Core.Models.Enums;
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
        private readonly BehaviorSubject<IEnumerable<FullName>> _markedItems = new(Enumerable.Empty<FullName>());
        private readonly List<IDisposable> _disposables = new();
        private bool disposed;

        public IObservable<IContainer?>? CurrentLocation { get; private set; }
        public IObservable<IItemViewModel?>? CurrentSelectedItem { get; private set; }
        public IObservable<IReadOnlyList<IItemViewModel>>? CurrentItems { get; private set; }
        public IObservable<IReadOnlyList<FullName>> MarkedItems { get; }

        public ITab? Tab { get; private set; }

        public TabViewModel(
            IServiceProvider serviceProvider,
            IItemNameConverterService itemNameConverterService,
            IAppState appState)
        {
            _serviceProvider = serviceProvider;
            _itemNameConverterService = itemNameConverterService;
            _appState = appState;

            MarkedItems = _markedItems.Select(e => e.ToList()).AsObservable();
        }

        public void Init(ITab tab)
        {
            CurrentLocation = tab.CurrentLocation.AsObservable();
            CurrentItems = tab.CurrentItems.Select(items => items.Select(MapItemToViewModel).ToList());
            CurrentSelectedItem = Observable.CombineLatest(
                CurrentItems,
                tab.CurrentSelectedItem,
                (currentItems, currentSelectedItemPath) => currentItems.FirstOrDefault(i => i.BaseItem?.FullName == currentSelectedItemPath?.Path));
            tab.CurrentLocation.Subscribe((_) => _markedItems.OnNext(Enumerable.Empty<FullName>()));

            Tab = tab;
        }

        private IItemViewModel MapItemToViewModel(IItem item, int index)
        {
            if (item is IContainer container)
            {
                var containerViewModel = _serviceProvider.GetRequiredService<IContainerViewModel>();
                InitIItemViewModel(containerViewModel, item);

                return containerViewModel;
            }
            else if (item is IFileElement fileElement)
            {
                var fileViewModel = _serviceProvider.GetRequiredService<IFileViewModel>();
                InitIItemViewModel(fileViewModel, item);
                fileViewModel.Size = fileElement.Size;

                return fileViewModel;
            }
            else if (item is IElement element)
            {
                var elementViewModel = _serviceProvider.GetRequiredService<IElementViewModel>();
                InitIItemViewModel(elementViewModel, item);

                return elementViewModel;
            }

            throw new ArgumentException($"{nameof(item)} is not {nameof(IContainer)} neighter {nameof(IElement)}");

            void InitIItemViewModel(IItemViewModel itemViewModel, IItem item)
            {
                itemViewModel.BaseItem = item;
                itemViewModel.DisplayName = _appState.SearchText.Select(s => _itemNameConverterService.GetDisplayName(item.DisplayName, s));
                itemViewModel.IsMarked = MarkedItems.Select(m => m.Contains(item.FullName));
                itemViewModel.IsSelected = MarkedItems.Select(m => m.Contains(item.FullName));
                itemViewModel.IsAlternative.OnNext(index % 2 == 0);
                itemViewModel.ViewMode = Observable.CombineLatest(itemViewModel.IsMarked, itemViewModel.IsSelected, itemViewModel.IsAlternative, GenerateViewMode);
                itemViewModel.Attributes = item.Attributes;
                itemViewModel.CreatedAt = item.CreatedAt;
            }
        }

        private ItemViewMode GenerateViewMode(bool isMarked, bool isSelected, bool sAlternative)
        => (isMarked, isSelected, sAlternative) switch
        {
            (true, true, _) => ItemViewMode.MarkedSelected,
            (true, false, true) => ItemViewMode.MarkedAlternative,
            (false, true, _) => ItemViewMode.Selected,
            (false, false, true) => ItemViewMode.Alternative,
            (true, false, false) => ItemViewMode.Marked,
            _ => ItemViewMode.Default
        };

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