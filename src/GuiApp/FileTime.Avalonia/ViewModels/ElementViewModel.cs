using FileTime.Core.Models;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.Services;
using MvvmGen;
using System.Collections.Generic;

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

        [PropertyInvalidate(nameof(IsSelected))]
        [PropertyInvalidate(nameof(IsAlternative))]
        public ItemViewMode ViewMode =>
            IsSelected
            ? ItemViewMode.Selected
            : IsAlternative
                ? ItemViewMode.Alternative
                : ItemViewMode.Default;

        public List<ItemNamePart> DisplayName => ItemNameConverterService.GetDisplayName(this);

        public ElementViewModel(IElement element, ItemNameConverterService itemNameConverterService) : this(itemNameConverterService)
        {
            Element = element;
        }

        public void InvalidateDisplayName() => OnPropertyChanged(nameof(DisplayName));
    }
}
