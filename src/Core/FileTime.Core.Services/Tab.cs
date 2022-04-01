using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.Core.Models;

namespace FileTime.Core.Services
{
    public class Tab : ITab
    {
        private readonly BehaviorSubject<IContainer?> _currentLocation = new(null);
        private readonly BehaviorSubject<IAbsolutePath?> _currentSelectedItem = new(null);
        public IObservable<IContainer?> CurrentLocation { get; }
        public IObservable<IEnumerable<IItem>> CurrentItems { get; }
        public IObservable<IAbsolutePath?> CurrentSelectedItem { get; }

        public Tab()
        {
            CurrentLocation = _currentLocation.AsObservable();
            CurrentItems = _currentLocation
                .Select(c =>
                    Observable.FromAsync(async () =>
                        c == null
                        ? Enumerable.Empty<IItem>()
                        : await c.Items
                            .ToAsyncEnumerable()
                            .SelectAwait(
                                async i =>
                                {
                                    try
                                    {
                                        //TODO: force create by AbsolutePath name
                                        return await i.ContentProvider.GetItemByFullNameAsync(i.Path);
                                    }
                                    catch { return null!; }
                                }

                            )
                            .Where(i => i != null)
                            .ToListAsync()
                    )
                )
                .Merge(Constants.MaximumObservableMergeOperations);
            CurrentSelectedItem = CurrentLocation.Select(GetSelectedItemByLocation).Merge(_currentSelectedItem).Throttle(TimeSpan.FromMilliseconds(500));
        }

        public void Init(IContainer currentLocation)
        {
            _currentLocation.OnNext(currentLocation);
        }

        private IAbsolutePath? GetSelectedItemByLocation(IContainer? currentLocation)
        {
            return currentLocation?.Items[0];
        }

        public void ChangeLocation(IContainer newLocation)
        {
            _currentLocation.OnNext(newLocation);
        }
    }
}