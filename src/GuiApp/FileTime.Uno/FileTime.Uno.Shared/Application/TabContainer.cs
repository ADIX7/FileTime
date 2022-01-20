using FileTime.Core.Components;
using FileTime.Core.Models;
using FileTime.Providers.Local;
using FileTime.Uno.ViewModels;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileTime.Uno.Application
{
    [ViewModel]
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

        [Property]
        [PropertyCallMethod(nameof(SelectedItemChanged))]
        private IItemViewModel _selectedItem;

        partial void OnInitialize()
        {
            Tab.CurrentLocationChanged += Tab_CurrentLocationChanged;
            Tab.CurrentSelectedItemChanged += Tab_CurrentSelectedItemChanged;

            CurrentLocation = new ContainerViewModel(Tab.CurrentLocation);
            var parent = Tab.CurrentLocation.GetParent();
            if (parent != null)
            {
                Parent = new ContainerViewModel(parent);
            }
            else
            {
                Parent = null;
            }

            UpdateCurrentSelectedItem();
        }

        private void Tab_CurrentLocationChanged(object sender, EventArgs e)
        {
            CurrentLocation = new ContainerViewModel(Tab.CurrentLocation);
            var parent = Tab.CurrentLocation.GetParent();
            if (parent != null)
            {
                Parent = new ContainerViewModel(parent);
            }
            else
            {
                Parent = null;
            }
        }

        private void Tab_CurrentSelectedItemChanged(object sender, EventArgs e)
        {
            UpdateCurrentSelectedItem();
        }

        private void UpdateCurrentSelectedItem()
        {
            IItemViewModel currentSelectenItem = null;
            if (Tab.CurrentSelectedItem == null)
            {
                SelectedItem = null;
                ChildContainer = null;
            }
            else
            {
                currentSelectenItem = _currentLocation.Items.Find(i => i.Item.Name == Tab.CurrentSelectedItem.Name);
                if (currentSelectenItem is ContainerViewModel currentSelectedContainer)
                {
                    SelectedItem = ChildContainer = currentSelectedContainer;
                }
                else if (currentSelectenItem is ElementViewModel element)
                {
                    ChildContainer = null;
                    SelectedItem = element;
                }
                else
                {
                    SelectedItem = null;
                    ChildContainer = null;
                }
            }

            foreach (var item in _currentLocation.Items)
            {
                item.IsSelected = item == currentSelectenItem;
            }
        }

        private void SelectedItemChanged()
        {
            Tab.CurrentSelectedItem = SelectedItem?.Item;
        }

        public void Open()
        {
            if (ChildContainer != null)
            {
                Tab.Open();
                UpdateCurrentSelectedItem();
            }
        }

        public void GoUp()
        {
            Tab.GoUp();
            UpdateCurrentSelectedItem();
        }

        public void MoveCursorDown()
        {
            Tab.SelectNextItem();
        }

        public void MoveCursorDownPage()
        {
            Tab.SelectNextItem(10);
        }

        public void MoveCursorUp()
        {
            Tab.SelectPreviousItem();
        }

        public void MoveCursorUpPage()
        {
            Tab.SelectPreviousItem(10);
        }

        public void MoveCursorToFirst()
        {
            Tab.SelectFirstItem();
        }

        public void MoveCursorToLast()
        {
            Tab.SelectLastItem();
        }

        public void GotToProvider()
        {
            Tab.GoToProvider();
        }

        public void GotToRoot()
        {
            Tab.GoToRoot();
        }

        public void GotToHome()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(Path.DirectorySeparatorChar, Constants.SeparatorChar);
            var resolvedPath = LocalContentProvider.GetByPath(path);
            if(resolvedPath is IContainer homeFolder)
            {
                Tab.OpenContainer(homeFolder);
            }
        }

        public void CreateContainer(string name)
        {
            Tab.CurrentLocation.CreateContainer(name);
        }
    }
}
