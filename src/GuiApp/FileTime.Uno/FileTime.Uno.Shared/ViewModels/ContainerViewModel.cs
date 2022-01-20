using FileTime.Core.Models;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileTime.Uno.ViewModels
{
    [ViewModel]
    public partial class ContainerViewModel : IItemViewModel
    {
        [Property]
        private IContainer _container;

        [Property]
        private bool _isSelected;

        public IItem Item => _container;

        //[Property]
        private List<ContainerViewModel> _containers;

        //[Property]
        private List<ElementViewModel> _elements;

        //[Property]
        private List<IItemViewModel> _items;

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

        public List<ContainerViewModel> Containers
        {
            get
            {
                if(_containers == null) Refresh();
                return _containers;
            }
            set
            {
                if(_containers != value)
                {
                    _containers = value;
                    OnPropertyChanged(nameof(_containers));
                }
            }
        }
        public List<ElementViewModel> Elements
        {
            get
            {
                if(_elements == null) Refresh();
                return _elements;
            }
            set
            {
                if(_elements != value)
                {
                    _elements = value;
                    OnPropertyChanged(nameof(_elements));
                }
            }
        }
        public List<IItemViewModel> Items
        {
            get
            {
                if(_items == null) Refresh();
                return _items;
            }
            set
            {
                if(_items != value)
                {
                    _items = value;
                    OnPropertyChanged(nameof(_items));
                }
            }
        }

        public ContainerViewModel(IContainer container)
        {
            Container = container;
            Container.Refreshed += Container_Refreshed;
        }

        private void Container_Refreshed(object sender, EventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            Containers = _container.Containers.Select(c => new ContainerViewModel(c)).ToList();
            Elements = _container.Elements.Select(e => new ElementViewModel(e)).ToList();

            Items = Containers.Cast<IItemViewModel>().Concat(Elements).ToList();

            for(var i = 0;i<Items.Count;i++)
            {
                Items[i].IsAlternative = i % 2 == 1;
            }
        }
    }
}
