using FileTime.Core.Models;
using FileTime.Avalonia.Models;
using System.Collections.Generic;

namespace FileTime.Avalonia.ViewModels
{
    public interface IItemViewModel
    {
        IItem Item { get; }
        IItem BaseItem { get; }
        bool IsSelected { get; set; }

        bool IsAlternative { get; set; }
        bool IsMarked { get; set; }
        ContainerViewModel? Parent{ get; set; }

        ItemViewMode ViewMode { get; }

        List<ItemNamePart> DisplayName { get; }

        void InvalidateDisplayName();
    }
}
