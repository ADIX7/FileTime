using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.Core.Services;

public interface ITab : IAsyncInitable<IContainer>, IDisposable
{
    IDeclarativeProperty<IContainer?> CurrentLocation { get; }
    IDeclarativeProperty<ObservableCollection<IItem>?> CurrentItems { get; }
    IDeclarativeProperty<AbsolutePath?> CurrentSelectedItem { get; }
    FullName? LastDeepestSelectedPath { get; }
    DeclarativeProperty<ItemOrdering?> Ordering { get; }

    Task SetCurrentLocation(IContainer newLocation);
    void AddItemFilter(ItemFilter filter);
    void RemoveItemFilter(ItemFilter filter);
    void RemoveItemFilter(string name);
    Task SetSelectedItem(AbsolutePath newSelectedItem);
    Task ForceSetCurrentLocation(IContainer newLocation);
    Task GoBackAsync();
    Task GoForwardAsync();
}