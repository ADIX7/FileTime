using System.Reactive.Subjects;
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
        private IItem? _baseItem;

        [Property]
        private IObservable<IReadOnlyList<ItemNamePart>>? _displayName;

        [Property]
        private IObservable<bool>? _isSelected;

        [Property]
        private IObservable<bool>? _isMarked;

        [Property]
        private IObservable<ItemViewMode> _viewMode;

        [Property]
        private DateTime? _createdAt;

        [Property]
        private string? _attributes;

        [Property]
        private BehaviorSubject<bool> _isAlternative = new(false);
    }
}