using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.Core.Models;

namespace FileTime.Core.Services
{
    public class Tab : ITab
    {
        private readonly BehaviorSubject<IContainer?> _currentLocation = new(null);
        private readonly BehaviorSubject<IAbsolutePath?> _currentSelectedItem = new(null);
        private readonly List<ItemsTransformator> _transformators = new();
        private IAbsolutePath? _currentSelectedItemCached;
        public IObservable<IContainer?> CurrentLocation { get; }
        public IObservable<IEnumerable<IItem>?> CurrentItems { get; }
        public IObservable<IAbsolutePath?> CurrentSelectedItem { get; }

        public Tab()
        {
            CurrentLocation = _currentLocation.DistinctUntilChanged().Do(_ => {; }).Publish(null).RefCount();
            CurrentItems =
                Observable.Merge(
                    CurrentLocation
                        .Where(c => c is not null)
                        .Select(c => c!.Items)
                        .Switch()
                        .Select(i => i == null ? Observable.Return<IEnumerable<IItem>?>(null) : Observable.FromAsync(async () => await MapItems(i)))
                        .Switch(),
                    CurrentLocation
                        .Where(c => c is null)
                        .Select(_ => Enumerable.Empty<IItem>())
                )
                .Publish(Enumerable.Empty<IItem>())
                .RefCount();

            CurrentSelectedItem = CurrentLocation
                .Select(GetSelectedItemByLocation)
                .Switch()
                .Merge(_currentSelectedItem)
                .DistinctUntilChanged()
                .Publish(null)
                .RefCount();

            CurrentSelectedItem.Subscribe(s => _currentSelectedItemCached = s);
        }

        private async Task<IEnumerable<IItem>> MapItems(IEnumerable<IAbsolutePath> items)
        {
            IEnumerable<IItem> resolvedItems = await items
                .ToAsyncEnumerable()
                .SelectAwait(async i => await i.ResolveAsync(true))
                .Where(i => i != null)
                .ToListAsync();

            return _transformators.Count == 0
                ? resolvedItems
                : (await _transformators
                        .ToAsyncEnumerable()
                        .Scan(resolvedItems, (acc, t) => new ValueTask<IEnumerable<IItem>>(t.Transformator(acc)))
                        .ToListAsync()
                    )
                    .SelectMany(t => t);
        }

        public void Init(IContainer currentLocation)
        {
            _currentLocation.OnNext(currentLocation);
        }

        private IObservable<IAbsolutePath?> GetSelectedItemByLocation(IContainer? currentLocation)
        {
            //TODO: 
            return currentLocation?.Items?.Select(i => i.FirstOrDefault()) ?? Observable.Return((IAbsolutePath?)null);
        }

        public void SetCurrentLocation(IContainer newLocation) => _currentLocation.OnNext(newLocation);

        public void SetSelectedItem(IAbsolutePath newSelectedItem) => _currentSelectedItem.OnNext(newSelectedItem);

        public void AddSelectedItemsTransformator(ItemsTransformator transformator) => _transformators.Add(transformator);
        public void RemoveSelectedItemsTransformator(ItemsTransformator transformator) => _transformators.Remove(transformator);
        public void RemoveSelectedItemsTransformatorByName(string name) => _transformators.RemoveAll(t => t.Name == name);

        public async Task OpenSelected()
        {
            if (_currentSelectedItemCached == null) return;
            var resolvedSelectedItem = await _currentSelectedItemCached.ContentProvider.GetItemByFullNameAsync(_currentSelectedItemCached.Path);

            if (resolvedSelectedItem is not IContainer resolvedContainer) return;
            SetCurrentLocation(resolvedContainer);
        }
    }
}