using FileTime.Core.Components;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FileTime.Avalonia.Application
{
    [ViewModel]
    public partial class AppState
    {
        [Property]
        private ObservableCollection<TabContainer> _tabs = new ObservableCollection<TabContainer>();

        [Property]
        private TabContainer _selectedTab;

        [Property]
        private ViewMode _viewMode;

        [Property]
        private string _rapidTravelText = "";

        partial void OnInitialize()
        {
            _tabs.CollectionChanged += (o, e) => SelectedTab ??= Tabs.Count > 0 ? Tabs[0] : null;
        }
    }
}
