
using DynamicData;
using FileTime.App.Core.Models;
using FileTime.Core.Models;
using FileTime.Core.Services;
using InitableService;

namespace FileTime.App.Core.ViewModels
{
    public interface ITabViewModel : IInitable<ITab, int>
    {
        ITab? Tab { get; }
        int TabNumber { get; }
        IObservable<bool> IsSelected { get; }
        IObservable<IContainer?> CurrentLocation { get; }
        IObservable<IItemViewModel?> CurrentSelectedItem { get; }
        IObservable<IObservable<IChangeSet<IItemViewModel>>?> CurrentItems { get; }
        IObservable<IChangeSet<IAbsolutePath>> MarkedItems { get; }
        IObservable<IObservable<IChangeSet<IItemViewModel>>?> SelectedsChildren { get; }
        IObservable<IObservable<IChangeSet<IItemViewModel>>?> ParentsChildren { get; }
        BindedCollection<IItemViewModel>? CurrentItemsCollection { get; }
        BindedCollection<IItemViewModel>? SelectedsChildrenCollection { get; }
        BindedCollection<IItemViewModel>? ParentsChildrenCollection { get; }
        IObservable<IReadOnlyCollection<IItemViewModel>?> CurrentItemsCollectionObservable { get; }
        void ClearMarkedItems();
        void RemoveMarkedItem(IAbsolutePath item);
        void AddMarkedItem(IAbsolutePath item);
        void ToggleMarkedItem(IAbsolutePath item);
    }
}