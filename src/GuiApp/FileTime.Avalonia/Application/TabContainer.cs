using AsyncEvent;
using FileTime.Core.Components;
using FileTime.Core.Models;
using FileTime.Providers.Local;
using FileTime.Avalonia.Services;
using FileTime.Avalonia.ViewModels;
using MvvmGen;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileTime.App.Core.Tab;
using System.Collections.Generic;

namespace FileTime.Avalonia.Application
{
    [ViewModel]
    [Inject(typeof(ItemNameConverterService))]
    [Inject(typeof(LocalContentProvider))]
    [Inject(typeof(Tab))]
    public partial class TabContainer : INewItemProcessor
    {
        [Property]
        private TabState _tabState;

        [Property]
        private ContainerViewModel _parent;

        [Property]
        private ContainerViewModel _currentLocation;

        [Property]
        private ContainerViewModel _childContainer;

        [Property]
        private int _tabNumber;

        [Property]
        private bool _isSelected;

        private IItemViewModel? _selectedItem;

        public IItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)// && value != null
                {
                    _selectedItem = value;

                    OnPropertyChanged("SelectedItem");
                    SelectedItemChanged();
                }
            }
        }

        partial void OnInitialize()
        {
            _tabState = new TabState(Tab);
        }

        public async Task Init(int tabNumber)
        {
            TabNumber = tabNumber;
            Tab.CurrentLocationChanged.Add(Tab_CurrentLocationChanged);
            Tab.CurrentSelectedItemChanged.Add(Tab_CurrentSelectedItemChanged);

            var currentLocation = await Tab.GetCurrentLocation();
            var parent = GenerateParent(currentLocation);
            CurrentLocation = new ContainerViewModel(this, parent, currentLocation, ItemNameConverterService);
            await CurrentLocation.Init();

            if (parent != null)
            {
                parent.ChildrenToAdopt.Add(CurrentLocation);
                Parent = parent;
                await Parent.Init();
            }
            else
            {
                Parent = null;
            }

            await UpdateCurrentSelectedItem();
        }

        private ContainerViewModel? GenerateParent(IContainer? container, bool recursive = false)
        {
            var parentContainer = container?.GetParent();
            if (parentContainer == null) return null;
            var parentParent = recursive ? GenerateParent(parentContainer.GetParent(), recursive) : null;

            var parent = new ContainerViewModel(this, parentParent, parentContainer, ItemNameConverterService);
            parentParent?.ChildrenToAdopt.Add(parent);
            return parent;
        }

        private async Task Tab_CurrentLocationChanged(object? sender, AsyncEventArgs e)
        {
            var currentLocation = await Tab.GetCurrentLocation();
            var parent = GenerateParent(currentLocation);
            CurrentLocation = new ContainerViewModel(this, parent, currentLocation, ItemNameConverterService);
            await CurrentLocation.Init();

            if (parent != null)
            {
                parent.ChildrenToAdopt.Add(CurrentLocation);
                Parent = parent;
                await Parent.Init();
            }
            else
            {
                Parent = null;
            }
        }

        private async Task Tab_CurrentSelectedItemChanged(object? sender, AsyncEventArgs e)
        {
            await UpdateCurrentSelectedItem();
        }

        public async Task UpdateCurrentSelectedItem()
        {
            try
            {
                var tabCurrentSelectenItem = await Tab.GetCurrentSelectedItem();
                IItemViewModel? currentSelectenItem = null;
                if (tabCurrentSelectenItem == null)
                {
                    SelectedItem = null;
                    ChildContainer = null;
                }
                else
                {
                    currentSelectenItem = (await _currentLocation.GetItems()).FirstOrDefault(i => i.Item.Name == tabCurrentSelectenItem.Name);
                    if (currentSelectenItem is ContainerViewModel currentSelectedContainer)
                    {
                        SelectedItem = currentSelectedContainer;
                        ChildContainer = currentSelectedContainer;
                    }
                    else if (currentSelectenItem is ElementViewModel element)
                    {
                        SelectedItem = element;
                        ChildContainer = null;
                    }
                    else
                    {
                        SelectedItem = null;
                        ChildContainer = null;
                    }
                }

                var items = await _currentLocation.GetItems();
                if (items != null && items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        var isSelected = item == currentSelectenItem;
                        item.IsSelected = isSelected;

                        if (isSelected)
                        {
                            var parent = item.Parent;
                            while (parent != null)
                            {
                                parent.IsSelected = true;
                                parent = parent.Parent;
                            }

                            try
                            {
                                var child = item;
                                while (child is ContainerViewModel containerViewModel && containerViewModel.Container.IsLoaded)
                                {
                                    var activeChildItem = await Tab.GetItemByLastPath(containerViewModel.Container);
                                    child = (await containerViewModel.GetItems()).FirstOrDefault(i => i.Item == activeChildItem);
                                    if (child != null)
                                    {
                                        child.IsSelected = true;
                                    }
                                }
                            }
                            catch
                            {
                                //INFO collection modified exception on: child = (await containerViewModel.GetItems()).FirstOrDefault(i => i.Item == activeChildItem);
                                //TODO: handle or error message
                            }
                        }
                    }
                }
                else
                {
                    var parent = _currentLocation;
                    while (parent != null)
                    {
                        parent.IsSelected = true;
                        parent = parent.Parent;
                    }
                }
            }
            catch 
            {
                //INFO collection modified exception on: currentSelectenItem = (await _currentLocation.GetItems()).FirstOrDefault(i => i.Item.Name == tabCurrentSelectenItem.Name);
                //TODO: handle or error message
            }
        }

        private async void SelectedItemChanged()
        {
            try
            {
                await Tab.SetCurrentSelectedItem(SelectedItem?.Item);
            }
            catch { }
        }

        public async Task Open()
        {
            if (ChildContainer != null)
            {
                await Tab.Open();
                await UpdateCurrentSelectedItem();
            }
        }

        public async Task GoUp()
        {
            await Tab.GoUp();
            await UpdateCurrentSelectedItem();
        }

        public async Task MoveCursorDown()
        {
            await Tab.SelectNextItem();
        }

        public async Task MoveCursorDownPage()
        {
            await Tab.SelectNextItem(10);
        }

        public async Task MoveCursorUp()
        {
            await Tab.SelectPreviousItem();
        }

        public async Task MoveCursorUpPage()
        {
            await Tab.SelectPreviousItem(10);
        }

        public async Task MoveCursorToFirst()
        {
            await Tab.SelectFirstItem();
        }

        public async Task MoveCursorToLast()
        {
            await Tab.SelectLastItem();
        }

        public async Task GotToProvider()
        {
            await Tab.GoToProvider();
        }

        public async Task GotToRoot()
        {
            await Tab.GoToRoot();
        }

        public async Task GotToHome()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(Path.DirectorySeparatorChar, Constants.SeparatorChar);
            var resolvedPath = await LocalContentProvider.GetByPath(path);
            if (resolvedPath is IContainer homeFolder)
            {
                await Tab.OpenContainer(homeFolder);
            }
        }

        public async Task CreateContainer(string name)
        {
            (await Tab.GetCurrentLocation())?.CreateContainer(name);
        }

        public async Task CreateElement(string name)
        {
            (await Tab.GetCurrentLocation())?.CreateElement(name);
        }

        public async Task OpenContainer(IContainer container)
        {
            await Tab.OpenContainer(container);
        }

        public async Task MarkCurrentItem()
        {
            await _tabState.MakrCurrentItem();
        }

        public async Task UpdateMarkedItems(ContainerViewModel containerViewModel)
        {
            if (containerViewModel == CurrentLocation && containerViewModel.Container.IsLoaded)
            {
                var selectedItems = TabState.GetCurrentMarkedItems(containerViewModel.Container);

                foreach (var item in await containerViewModel.GetItems())
                {
                    item.IsMarked = selectedItems.Any(c => c.Path == item.Item.FullName);
                }
            }
        }
    }
}
