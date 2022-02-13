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
    public partial class ContainerViewModel : IItemViewModel
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

        public Task Containers => GetContainers();
        public Task Elements => GetElements();
        public Task Items => GetItems();

        public async Task<ObservableCollection<ContainerViewModel>> GetContainers(CancellationToken token = default)
        {
            if (!_isInitialized) await Task.Run(async () => await Refresh(false, token: token), token);
            return _containers;
        }

        public async Task<ObservableCollection<ElementViewModel>> GetElements(CancellationToken token = default)
        {
            if (!_isInitialized) await Task.Run(async () => await Refresh(false, token: token), token);
            return _elements;
        }

        public async Task<ObservableCollection<IItemViewModel>> GetItems(CancellationToken token = default)
        {
            if (!_isInitialized) await Task.Run(async () => await Refresh(false, token: token), token);
            return _items;
        }

        private void SetContainers(ObservableCollection<ContainerViewModel> value)
        {
            if (value != _containers)
            {
                _containers = value;
                OnPropertyChanged(nameof(Containers));
            }
        }

        private void SetElements(ObservableCollection<ElementViewModel> value)
        {
            if (value != _elements)
            {
                _elements = value;
                OnPropertyChanged(nameof(Elements));
            }
        }

        private void SetItems(ObservableCollection<IItemViewModel> value)
        {
            if (value != _items)
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
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
            if (_isInitialized) return;
            await Refresh(initializeChildren, token: token);
        }

        private async Task Container_Refreshed(object? sender, AsyncEventArgs e, CancellationToken token = default)
        {
            await Refresh(false, false, token: token);
        }

        [Obsolete($"Use the parametrizable version of {nameof(Refresh)}.")]
        private async Task Refresh()
        {
            await Refresh(true, silent: true);
        }
        private async Task Refresh(bool initializeChildren, bool alloweReuse = true, bool silent = false, CancellationToken token = default)
        {
            if (_isRefreshing) return;

            _isInitialized = true;

            Exceptions = new List<Exception>();
            try
            {
                _isRefreshing = true;

                List<ContainerViewModel> newContainers = new List<ContainerViewModel>();
                List<ElementViewModel> newElements = new List<ElementViewModel>();

                if (await _container.GetContainers() is IReadOnlyList<IContainer> containers)
                {
                    foreach (var container in containers)
                    {
                        newContainers.Add(await AdoptOrReuseOrCreateItem(container, alloweReuse, (c2) => new ContainerViewModel(_newItemProcessor, this, c2, ItemNameConverterService)));
                    }
                }

                if (await _container.GetElements() is IReadOnlyList<IElement> elements)
                {
                    foreach (var element in elements)
                    {
                        var generator = async (IElement e) =>
                        {
                            var element = new ElementViewModel(e, this, ItemNameConverterService);
                            await element.Init();
                            return element;
                        };

                        newElements.Add(await AdoptOrReuseOrCreateItem(element, alloweReuse, generator));
                    }
                }

                if (token.IsCancellationRequested) return;

                Exceptions = new List<Exception>(_container.Exceptions);

                if (initializeChildren)
                {
                    foreach (var container in newContainers)
                    {
                        if (token.IsCancellationRequested) return;
                        await container.Init(false, token);
                    }
                }

                if (silent)
                {
                    _containers = new ObservableCollection<ContainerViewModel>(newContainers);
                    _elements = new ObservableCollection<ElementViewModel>(newElements);
                    _items = new ObservableCollection<IItemViewModel>(newContainers.Cast<IItemViewModel>().Concat(newElements));
                }
                else
                {
                    SetContainers(new ObservableCollection<ContainerViewModel>(newContainers));
                    SetElements(new ObservableCollection<ElementViewModel>(newElements));
                    SetItems(new ObservableCollection<IItemViewModel>(newContainers.Cast<IItemViewModel>().Concat(newElements)));
                }

                for (var i = 0; i < _items.Count; i++)
                {
                    _items[i].IsAlternative = i % 2 == 1;
                }
            }
            catch (Exception e)
            {
                _exceptions.Add(e);
            }

            await _newItemProcessor.UpdateMarkedItems(this);

            _isRefreshing = false;
        }

        private int GetNewItemPosition<TItem, T>(TItem itemToAdd, IList<T> items) where TItem : IItemViewModel where T : IItemViewModel
        {
            var i = 0;
            for (; i < items.Count; i++)
            {
                var item = items[i];
                if (item is TItem && itemToAdd.Item.Name.CompareTo(item.Item.Name) < 0)
                {
                    return i - 1;
                }
            }

            return i;
        }

        private async Task<TResult> AdoptOrReuseOrCreateItem<T, TResult>(T item, bool allowResuse, Func<T, TResult> generator) where T : class, IItem
        {
            return await AdoptOrReuseOrCreateItem(item, allowResuse, Helper);

            Task<TResult> Helper(T item)
            {
                return Task.FromResult(generator(item));
            }
        }
        private async Task<TResult> AdoptOrReuseOrCreateItem<T, TResult>(T item, bool allowResuse, Func<T, Task<TResult>> generator) where T : class, IItem
        {
            var itemToAdopt = ChildrenToAdopt.Find(i => i.Item == item);
            if (itemToAdopt is TResult itemViewModel) return itemViewModel;

            if (allowResuse)
            {
                var existingViewModel = _items?.FirstOrDefault(i => i.Item == item);
                if (existingViewModel is TResult itemViewModelToReuse) return itemViewModelToReuse;
            }

            return await generator(item);
        }

        public void Unload(bool recursive = true, bool unloadParent = true, bool unloadEvents = false)
        {
            _isInitialized = false;
            if (recursive)
            {
                foreach (var container in _containers)
                {
                    container.Unload(true, false, true);
                    container.Dispose();
                    container.ChildrenToAdopt.Clear();
                }
            }

            if (unloadParent)
            {
                var parent = Parent;
                while (parent != null)
                {
                    var lastParent = parent;
                    parent = parent.Parent;
                    lastParent.Unload();
                }
            }

            if (unloadEvents)
            {
                Container.Refreshed.Remove(Container_Refreshed);
            }

            _containers.Clear();
            _elements.Clear();
            _items.Clear();
        }

        private void Dispose()
        {
            if (!_disposed)
            {
                Container.Refreshed.Remove(Container_Refreshed);
            }
            _disposed = true;
        }
    }
}
