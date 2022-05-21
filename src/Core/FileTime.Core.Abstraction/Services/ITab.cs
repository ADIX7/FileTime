using DynamicData;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.Core.Services;

public interface ITab : IInitable<IContainer>
{
    IObservable<IContainer?> CurrentLocation { get; }
    IObservable<AbsolutePath?> CurrentSelectedItem { get; }
    IObservable<IObservable<IChangeSet<IItem>>?> CurrentItems { get; }

    void SetCurrentLocation(IContainer newLocation);
    void AddItemFilter(ItemFilter filter);
    void RemoveItemFilter(ItemFilter filter);
    void RemoveItemFilter(string name);
    void SetSelectedItem(AbsolutePath newSelectedItem);
}