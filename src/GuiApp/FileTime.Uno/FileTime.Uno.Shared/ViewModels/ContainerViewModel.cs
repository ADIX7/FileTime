using AsyncEvent;
using FileTime.Core.Models;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTime.Uno.ViewModels
{
    [ViewModel]
    public partial class ContainerViewModel : IItemViewModel
    {
        private bool isRefreshing;

        [Property]
        private IContainer _container;

        [Property]
        private bool _isSelected;

        public IItem Item => _container;

        //[Property]
        private readonly ObservableCollection<ContainerViewModel> _containers = new ObservableCollection<ContainerViewModel>();

        //[Property]
        private readonly ObservableCollection<ElementViewModel> _elements = new ObservableCollection<ElementViewModel>();

        //[Property]
        private readonly ObservableCollection<IItemViewModel> _items = new ObservableCollection<IItemViewModel>();

        [Property]
        private bool _isAlternative;


        [PropertyInvalidate(nameof(IsSelected))]
        [PropertyInvalidate(nameof(IsAlternative))]
        public ItemViewMode ViewMode =>
            IsSelected
            ? ItemViewMode.Selected
            : IsAlternative
                ? ItemViewMode.Alternative
                : ItemViewMode.Default;

        public ObservableCollection<ContainerViewModel> Containers
        {
            get
            {
                if (_containers == null) Task.Run(Refresh);
                return _containers;
            }
        }
        public ObservableCollection<ElementViewModel> Elements
        {
            get
            {
                if (_elements == null) Task.Run(Refresh);
                return _elements;
            }
        }
        public ObservableCollection<IItemViewModel> Items
        {
            get
            {
                if (_items == null) Task.Run(Refresh);
                return _items;
            }
        }

        public ContainerViewModel(IContainer container) : this()
        {
            Container = container;
            Container.Refreshed.Add(Container_Refreshed);
        }

        public async Task Init(bool initializeChildren = true)
        {
            await Refresh(initializeChildren);
        }

        private async Task Container_Refreshed(object sender, AsyncEventArgs e)
        {
            await Refresh(false);
        }

        private async Task Refresh()
        {
            await Refresh(true);
        }
        private async Task Refresh(bool initializeChildren)
        {
            if (isRefreshing) return;

            try
            {
                isRefreshing = true;

                var containers = (await _container.GetContainers()).Select(c => new ContainerViewModel(c)).ToList();
                var elements = (await _container.GetElements()).Select(e => new ElementViewModel(e)).ToList();

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

            isRefreshing = false;
        }
    }
}
