using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.Services;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileTime.Avalonia.Application;
using System.Threading;

namespace FileTime.Avalonia.ViewModels
{
    [ViewModel]
    [Inject(typeof(ItemNameConverterService))]
    public partial class ContainerViewModel : IItemViewModel, IDisposable
    {
        private bool _disposed;
        private bool _isRefreshing;
        private bool _isInitialized;
        private readonly INewItemProcessor _newItemProcessor;

        [Property]
        private IContainer _container;

        [Property]
        private bool _isSelected;

        [Property]
        private bool _isAlternative;

        [Property]
        private bool _isMarked;

        [Property]
        private ContainerViewModel? _parent;

        [Property]
        private List<Exception> _exceptions;

        public IItem Item => _container;

        private ObservableCollection<ContainerViewModel> _containers = new();

        private ObservableCollection<ElementViewModel> _elements = new();

        private ObservableCollection<IItemViewModel> _items = new();

        public List<IItemViewModel> ChildrenToAdopt { get; } = new List<IItemViewModel>();

        [PropertyInvalidate(nameof(IsSelected))]
        [PropertyInvalidate(nameof(IsAlternative))]
        [PropertyInvalidate(nameof(IsMarked))]
        public ItemViewMode ViewMode =>
            (IsMarked, IsSelected, IsAlternative) switch
            {
                (true, true, _) => ItemViewMode.MarkedSelected,
                (true, false, true) => ItemViewMode.MarkedAlternative,
                (false, true, _) => ItemViewMode.Selected,
                (false, false, true) => ItemViewMode.Alternative,
                (true, false, false) => ItemViewMode.Marked,
                _ => ItemViewMode.Default
            };

        public List<ItemNamePart> DisplayName => ItemNameConverterService.GetDisplayName(this);

        [Obsolete($"This property is for databinding only, use {nameof(GetContainers)} method instead.")]
        public ObservableCollection<ContainerViewModel> Containers
        {
            get
            {
                if (!_isInitialized) Task.Run(Refresh);
                return _containers;
            }
            set
            {
                if (value != _containers)
                {
                    _containers = value;
                    OnPropertyChanged(nameof(Containers));
                }
            }
        }

        [Obsolete($"This property is for databinding only, use {nameof(GetElements)} method instead.")]
        public ObservableCollection<ElementViewModel> Elements
        {
            get
            {
                if (!_isInitialized) Task.Run(Refresh);
                return _elements;
            }
            set
            {
                if (value != _elements)
                {
                    _elements = value;
                    OnPropertyChanged(nameof(Elements));
                }
            }
        }
        [Obsolete($"This property is for databinding only, use {nameof(GetItems)} method instead.")]
        public ObservableCollection<IItemViewModel> Items
        {
            get
            {
                if (!_isInitialized) Task.Run(Refresh);
                return _items;
            }
            set
            {
                if (value != _items)
                {
                    _items = value;
                    OnPropertyChanged(nameof(Items));
                }
            }
        }

        public ContainerViewModel(INewItemProcessor newItemProcessor, ContainerViewModel? parent, IContainer container, ItemNameConverterService itemNameConverterService) : this(itemNameConverterService)
        {
            _newItemProcessor = newItemProcessor;
            Parent = parent;

            Container = container;
            Container.Refreshed.Add(Container_Refreshed);
        }

        public void InvalidateDisplayName() => OnPropertyChanged(nameof(DisplayName));

        public async Task Init(bool initializeChildren = true, CancellationToken token = default)
        {
            await Refresh(initializeChildren, token);
        }

        private async Task Container_Refreshed(object? sender, AsyncEventArgs e, CancellationToken token = default)
        {
            await Refresh(false, token);
        }

        [Obsolete($"Use the parametrizable version of {nameof(Refresh)}.")]
        private async Task Refresh()
        {
            await Refresh(true);
        }
        private async Task Refresh(bool initializeChildren, CancellationToken token = default)
        {
            if (_isRefreshing) return;

            _isInitialized = true;

            Exceptions = new List<Exception>();
            try
            {
                _isRefreshing = true;

                var containers = (await _container.GetContainers())!.Select(c => AdoptOrReuseOrCreateItem(c, (c2) => new ContainerViewModel(_newItemProcessor, this, c2, ItemNameConverterService))).ToList();
                var elements = (await _container.GetElements())!.Select(e => AdoptOrReuseOrCreateItem(e, (e2) => new ElementViewModel(e2, this, ItemNameConverterService))).ToList();

                if (token.IsCancellationRequested) return;

                Exceptions = new List<Exception>(_container.Exceptions);

                foreach (var containerToRemove in _containers.Except(containers))
                {
                    containerToRemove?.Dispose();
                }

                if (initializeChildren)
                {
                    foreach (var container in containers)
                    {
                        if (token.IsCancellationRequested) return;
                        await container.Init(false, token);
                    }
                }

                for (var i = 0; i < _items.Count; i++)
                {
                    _items[i].IsAlternative = i % 2 == 1;
                }

                Containers = new ObservableCollection<ContainerViewModel>(containers);
                Elements = new ObservableCollection<ElementViewModel>(elements);
                Items = new ObservableCollection<IItemViewModel>(containers.Cast<IItemViewModel>().Concat(elements));
            }
            catch (Exception e)
            {
                _exceptions.Add(e);
            }

            await _newItemProcessor.UpdateMarkedItems(this);

            _isRefreshing = false;
        }

        private TResult AdoptOrReuseOrCreateItem<T, TResult>(T item, Func<T, TResult> generator) where T : class, IItem
        {
            var itemToAdopt = ChildrenToAdopt.Find(i => i.Item == item);
            if (itemToAdopt is TResult itemViewModel) return itemViewModel;

            var existingViewModel = _items?.FirstOrDefault(i => i.Item == item);
            if (existingViewModel is TResult itemViewModelToReuse) return itemViewModelToReuse;

            return generator(item);
        }

        public void Unload(bool recursive = true)
        {
            _isInitialized = false;
            if (recursive)
            {
                foreach (var container in _containers)
                {
                    container.Unload(true);
                    container.Dispose();
                    container.ChildrenToAdopt.Clear();
                }
            }

            _containers = new ObservableCollection<ContainerViewModel>();
            _elements = new ObservableCollection<ElementViewModel>();
            _items = new ObservableCollection<IItemViewModel>();
        }

        public async Task<ObservableCollection<ContainerViewModel>> GetContainers(CancellationToken token = default)
        {
            if (!_isInitialized) await Task.Run(async () => await Refresh(false, token), token);
            return _containers;
        }

        public async Task<ObservableCollection<ElementViewModel>> GetElements(CancellationToken token = default)
        {
            if (!_isInitialized) await Task.Run(async () => await Refresh(false, token), token);
            return _elements;
        }

        public async Task<ObservableCollection<IItemViewModel>> GetItems(CancellationToken token = default)
        {
            if (!_isInitialized) await Task.Run(async () => await Refresh(false, token), token);
            return _items;
        }

        ~ContainerViewModel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                Container.Refreshed.Remove(Container_Refreshed);
            }
            _disposed = true;
        }
    }
}
