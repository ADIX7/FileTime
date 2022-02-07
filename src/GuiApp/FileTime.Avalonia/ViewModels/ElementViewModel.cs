using FileTime.Core.Models;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.Services;
using MvvmGen;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileTime.Avalonia.ViewModels
{
    [ViewModel]
    [Inject(typeof(ItemNameConverterService))]
    public partial class ElementViewModel : IItemViewModel
    {
        public IItem Item => _element;

        [Property]
        private IElement _element;

        [Property]
        private bool _isSelected;

        [Property]
        private bool _isAlternative;

        [Property]
        private bool _isMarked;

        [Property]
        private ContainerViewModel? _parent;

        [Property]
        private long _size;

        [PropertyInvalidate(nameof(IsSelected))]
        [PropertyInvalidate(nameof(IsAlternative))]
        [PropertyInvalidate(nameof(IsMarked))]
        public ItemViewMode ViewMode =>
            (IsMarked, IsSelected, IsAlternative) switch
            {
                (true, true, _) => ItemViewMode.MarkedSelected,
                (true, false, true) => ItemViewMode.MarkedAlternative,
                (false, true, _) => ItemViewMode.Selected,
                (false, false, true) => ItemViewMode.Alternative,
                (true, false, false) => ItemViewMode.Marked,
                _ => ItemViewMode.Default
            };

        public List<ItemNamePart> DisplayName => ItemNameConverterService.GetDisplayName(this);

        public ElementViewModel(IElement element, ContainerViewModel parent, ItemNameConverterService itemNameConverterService) : this(itemNameConverterService)
        {
            Element = element;
            Parent = parent;
        }

        public void InvalidateDisplayName() => OnPropertyChanged(nameof(DisplayName));

        public async Task Init()
        {
            Size = await _element.GetElementSize();
        }
    }
}
