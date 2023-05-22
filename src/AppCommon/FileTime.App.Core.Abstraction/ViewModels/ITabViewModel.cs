
using DynamicData;
using FileTime.Core.Models;
using FileTime.Core.Services;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface ITabViewModel : IInitable<ITab, int>, IDisposable
{
    ITab? Tab { get; }
    int TabNumber { get; }
    IObservable<bool> IsSelected { get; }
    IObservable<IContainer?> CurrentLocation { get; }
    IObservable<IItemViewModel?> CurrentSelectedItem { get; }
    IObservable<IObservable<IChangeSet<IItemViewModel, string>>?> CurrentItems { get; }
    IObservable<IChangeSet<FullName>> MarkedItems { get; }
    IObservable<IObservable<IChangeSet<IItemViewModel, string>>?> SelectedsChildren { get; }
    IObservable<IObservable<IChangeSet<IItemViewModel, string>>?> ParentsChildren { get; }
    BindedCollection<IItemViewModel, string>? CurrentItemsCollection { get; }
    BindedCollection<IItemViewModel, string>? SelectedsChildrenCollection { get; }
    BindedCollection<IItemViewModel, string>? ParentsChildrenCollection { get; }
    IObservable<IReadOnlyCollection<IItemViewModel>?> CurrentItemsCollectionObservable { get; }
    IObservable<IReadOnlyCollection<IItemViewModel>?> ParentsChildrenCollectionObservable { get; }
    IObservable<IReadOnlyCollection<IItemViewModel>?> SelectedsChildrenCollectionObservable { get; }
    IContainer? CachedCurrentLocation { get; }

    void ClearMarkedItems();
    void RemoveMarkedItem(FullName fullName);
    void AddMarkedItem(FullName fullName);
    void ToggleMarkedItem(FullName fullName);
}