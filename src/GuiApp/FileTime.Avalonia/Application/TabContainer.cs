using AsyncEvent;
using FileTime.Core.Components;
using FileTime.Core.Models;
using FileTime.Providers.Local;
using FileTime.Avalonia.Services;
using FileTime.Avalonia.ViewModels;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTime.Avalonia.Application
{
    [ViewModel]
    [Inject(typeof(ItemNameConverterService))]
    [Inject(typeof(LocalContentProvider))]
    [Inject(typeof(Tab))]
    public partial class TabContainer
    {
        [Property]
        private ContainerViewModel _parent;

        [Property]
        private ContainerViewModel _currentLocation;

        [Property]
        private ContainerViewModel _childContainer;

        private IItemViewModel? _selectedItem;

        public IItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value && value != null)
                {
                    _selectedItem = value;
                    OnPropertyChanged("SelectedItem");
                    SelectedItemChanged();
                }
            }
        }

        public async Task Init()
        {
            Tab.CurrentLocationChanged.Add(Tab_CurrentLocationChanged);
            Tab.CurrentSelectedItemChanged.Add(Tab_CurrentSelectedItemChanged);

            CurrentLocation = new ContainerViewModel(await Tab.GetCurrentLocation(), ItemNameConverterService);
            await CurrentLocation.Init();

            var parent = (await Tab.GetCurrentLocation()).GetParent();
            if (parent != null)
            {
                Parent = new ContainerViewModel(parent, ItemNameConverterService);
                await Parent.Init();
            }
            else
            {
                Parent = null;
            }

            await UpdateCurrentSelectedItem();
        }

        private async Task Tab_CurrentLocationChanged(object? sender, AsyncEventArgs e)
        {
            var currentLocation = await Tab.GetCurrentLocation();
            CurrentLocation = new ContainerViewModel(currentLocation, ItemNameConverterService);
            await CurrentLocation.Init();

            var parent = currentLocation.GetParent();
            if (parent != null)
            {
                Parent = new ContainerViewModel(parent, ItemNameConverterService);
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

        private async Task UpdateCurrentSelectedItem()
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
            foreach (var item in items)
            {
                item.IsSelected = item == currentSelectenItem;
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

        public async Task OpenContainer(IContainer container)
        {
            await Tab.OpenContainer(container);
        }
    }
}
