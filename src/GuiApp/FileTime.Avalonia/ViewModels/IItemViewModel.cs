using FileTime.Core.Models;
using FileTime.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Avalonia.ViewModels
{
    public interface IItemViewModel
    {
        IItem Item { get; }
        bool IsSelected { get; set; }

        bool IsAlternative { get; set; }

        ItemViewMode ViewMode { get; }

        List<ItemNamePart> DisplayName { get; }

        void InvalidateDisplayName();
    }
}
