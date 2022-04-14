using FileTime.Core.Models;
using InitableService;

namespace FileTime.Core.Services
{
    public interface ITab : IInitable<IContainer>
    {
        IObservable<IContainer?> CurrentLocation { get; }
        IObservable<IAbsolutePath?> CurrentSelectedItem { get; }
        IObservable<IEnumerable<IItem>?> CurrentItems { get; }

        void SetCurrentLocation(IContainer newLocation);
        void AddSelectedItemsTransformator(ItemsTransformator transformator);
        void RemoveSelectedItemsTransformator(ItemsTransformator transformator);
        void RemoveSelectedItemsTransformatorByName(string name);
        void SetSelectedItem(IAbsolutePath newSelectedItem);
    }
}