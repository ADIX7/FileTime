using FileTime.Core.Models;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Uno.ViewModels
{
    [ViewModel]
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

        public ElementViewModel(IElement element)
        {
            Element = element;
        }
    }
}
