using FileTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Uno.ViewModels
{
    public interface IItemViewModel
    {
        IItem Item { get; }
        bool IsSelected { get; set; }

        bool IsAlternative { get; set; }

        ItemViewMode ViewMode { get; }
    }
}
