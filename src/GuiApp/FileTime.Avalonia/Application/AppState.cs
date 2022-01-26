using FileTime.Core.Components;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Avalonia.Application
{
    [ViewModel]
    public partial class AppState
    {
        [Property]
        [PropertyCallMethod(nameof(TabsChanged))]
        private List<TabContainer> _tabs = new List<TabContainer>();

        [Property]
        private TabContainer _selectedTab;

        [Property]
        private ViewMode _viewMode;

        [Property]
        private string _rapidTravelText = "";

        private void TabsChanged()
        {
            SelectedTab ??= Tabs[0];
        }
    }
}
