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

namespace FileTime.Avalonia.ViewModels
{
    [ViewModel]
    [Inject(typeof(ItemNameConverterService))]
    public partial class ContainerViewModel : IItemViewModel
    {
        private bool _isRefreshing;
        private bool _isInitialized;
        private INewItemProcessor _newItemProcessor;

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

        public IItem Item => _container;

        private readonly ObservableCollection<ContainerViewModel> _containers = new ObservableCollection<ContainerViewModel>();

        private readonly ObservableCollection<ElementViewModel> _elements = new ObservableCollection<ElementViewModel>();

        private readonly ObservableCollection<IItemViewModel> _items = new ObservableCollection<IItemViewModel>();

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

        [Obsolete]
        public ObservableCollection<ContainerViewModel> Containers
        {
            get
            {
                if (!_isInitialized) Task.Run(Refresh);
                return _containers;
            }
        }

        [Obsolete]
        public ObservableCollection<ElementViewModel> Elements
        {
            get
            {
                if (!_isInitialized) Task.Run(Refresh);
                return _elements;
            }
        }

        [Obsolete]
        public ObservableCollection<IItemViewModel> Items
        {
            get
            {
                if (!_isInitialized) Task.Run(Refresh);
                return _items;
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

        public async Task Init(bool initializeChildren = true)
        {
            await Refresh(initializeChildren);
        }

        private async Task Container_Refreshed(object? sender, AsyncEventArgs e)
        {
            await Refresh(false);
        }

        private async Task Refresh()
        {
            await Refresh(true);
        }
        private async Task Refresh(bool initializeChildren)
        {
            if (_isRefreshing) return;

            _isInitialized = true;

            try
            {
                _isRefreshing = true;

                var containers = (await _container.GetContainers()).Select(c => AdoptOrCreateItem(c, (c2) => new ContainerViewModel(_newItemProcessor, this, c2, ItemNameConverterService))).ToList();
                var elements = (await _container.GetElements()).Select(e => AdoptOrCreateItem(e, (e2) => new ElementViewModel(e2, this, ItemNameConverterService))).ToList();

                _containers.Clear();
                _elements.Clear();
                _items.Clear();

                foreach (var container in containers)
                {
                    if (initializeChildren) await container.Init(false);

                    _containers.Add(container);
                    _items.Add(container);
                }

                foreach (var element in elements)
                {
                    _elements.Add(element);
                    _items.Add(element);
                }

                for (var i = 0; i < _items.Count; i++)
                {
                    _items[i].IsAlternative = i % 2 == 1;
                }
            }
            catch { }

            await _newItemProcessor.UpdateMarkedItems(this);

            _isRefreshing = false;
        }

        private TResult AdoptOrCreateItem<T, TResult>(T item, Func<T, TResult> generator) where T : IItem
        {
            var itemToAdopt = ChildrenToAdopt.Find(i => i.Item.Name == item.Name);
            if (itemToAdopt is TResult itemViewModel) return itemViewModel;

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
                    container.ChildrenToAdopt.Clear();
                }
            }

            _containers.Clear();
            _elements.Clear();
            _items.Clear();
        }

        public async Task<ObservableCollection<ContainerViewModel>> GetContainers()
        {
            if (!_isInitialized) await Task.Run(Refresh);
            return _containers;
        }

        public async Task<ObservableCollection<ElementViewModel>> GetElements()
        {
            if (!_isInitialized) await Task.Run(Refresh);
            return _elements;
        }

        public async Task<ObservableCollection<IItemViewModel>> GetItems()
        {
            if (!_isInitialized) await Task.Run(Refresh);
            return _items;
        }
    }
}
