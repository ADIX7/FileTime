using FileTime.Uno.Application;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Uno.ViewModels
{
    [ViewModel]
    [Inject(typeof(TabContainer), "Tab", PropertyAccessModifier = AccessModifier.Public)]
    [Inject(typeof(int), "TabNumber", PropertyAccessModifier = AccessModifier.Public)]
    public partial class TabControlViewModel
    {
        [Property]
        private bool _isSelected;
    }
}
