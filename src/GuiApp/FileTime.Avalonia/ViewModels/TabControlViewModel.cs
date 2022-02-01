using FileTime.Avalonia.Application;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Avalonia.ViewModels
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
