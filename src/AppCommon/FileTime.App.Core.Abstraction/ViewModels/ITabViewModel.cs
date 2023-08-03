
using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.App.Core.Models;
using FileTime.Core.Models;
using FileTime.Core.Services;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface ITabViewModel : IInitable<ITab, int>, IDisposable
{
    ITab? Tab { get; }
    int TabNumber { get; }
    IObservable<bool> IsSelected { get; }
    IDeclarativeProperty<IContainer?> CurrentLocation { get; }
    IDeclarativeProperty<IItemViewModel?> CurrentSelectedItem { get; }
    IDeclarativeProperty<IContainerViewModel?> CurrentSelectedItemAsContainer { get; }
    IDeclarativeProperty<ObservableCollection<IItemViewModel>?> CurrentItems { get; }
    IDeclarativeProperty<ObservableCollection<FullName>> MarkedItems { get; }
    IDeclarativeProperty<ObservableCollection<IItemViewModel>> SelectedsChildren { get; }
    IDeclarativeProperty<ObservableCollection<IItemViewModel>> ParentsChildren { get; }
    DeclarativeProperty<ItemOrdering?> Ordering { get; }

    void ClearMarkedItems();
    void RemoveMarkedItem(FullName fullName);
    void AddMarkedItem(FullName fullName);
    void ToggleMarkedItem(FullName fullName);
}