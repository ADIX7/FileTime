using FileTime.Core.Components;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Uno.Application
{
    [ViewModel]
    public partial class AppState
    {
        [Property]
        [PropertyCallMethod(nameof(TabsChanged))]
        private List<TabContainer> _tabs = new List<TabContainer>();

        [Property]
        private TabContainer _selectedTab;
        private void TabsChanged()
        {
            SelectedTab ??= Tabs[0];
        }
    }
}
