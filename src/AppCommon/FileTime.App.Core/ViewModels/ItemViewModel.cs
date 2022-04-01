using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;
using MvvmGen;

namespace FileTime.App.Core.ViewModels
{
    [ViewModel]
    public abstract partial class ItemViewModel : IItemViewModel
    {
        [Property]
        private IItem? _item;

        [Property]
        private IObservable<IReadOnlyList<ItemNamePart>>? _displayName;

        [Property]
        private IObservable<bool>? _isSelected;

        [Property]
        private IObservable<bool>? _isMarked;

        [Property]
        private ItemViewMode _viewMode;
    }
}