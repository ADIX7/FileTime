using FileTime.Core.Models;
using InitableService;

namespace FileTime.Core.Services
{
    public interface ITab : IInitable<IContainer>
    {
        IObservable<IContainer?> CurrentLocation { get; }
        IObservable<IAbsolutePath?> CurrentSelectedItem { get; }
        IObservable<IEnumerable<IItem>> CurrentItems { get; }

        void ChangeLocation(IContainer newLocation);
    }
}