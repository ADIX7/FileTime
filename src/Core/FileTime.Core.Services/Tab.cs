using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
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
        public IObservable<IObservable<IChangeSet<IItem>>?> CurrentItems { get; }
        public IObservable<IAbsolutePath?> CurrentSelectedItem { get; }

        public Tab()
        {
            CurrentLocation = _currentLocation.DistinctUntilChanged().Publish(null).RefCount();
            CurrentItems =
                Observable.Merge(
                    CurrentLocation
                        .Where(c => c is not null)
                        .Select(c => c!.Items)
                        .Switch()
                        .Select(items => items?.TransformAsync(MapItem)),
                    CurrentLocation
                        .Where(c => c is null)
                        .Select(_ => (IObservable<IChangeSet<IItem>>?)null)
                )
                .Publish((IObservable<IChangeSet<IItem>>?)null)
                .RefCount();

            CurrentSelectedItem =
                Observable.CombineLatest(
                    CurrentItems
                        .Select(c =>
                            c == null
                            ? Observable.Return<IReadOnlyCollection<IItem>?>(null)
                            : c.ToCollection()
                        )
                        .Switch(),
                    _currentSelectedItem,
                    (items, selected) =>
                    {
                        if (selected != null && (items?.Any(i => i.FullName == selected.Path) ?? true)) return selected;
                        if (items == null || items.Count == 0) return null;

                        return GetSelectedItemByItems(items);
                    }
                )
                .DistinctUntilChanged()
                .Publish(null)
                .RefCount();

            CurrentSelectedItem.Subscribe(s =>
            {
                _currentSelectedItemCached = s;
                _currentSelectedItem.OnNext(s);
            });
        }

        private async Task<IItem> MapItem(IAbsolutePath item) => await item.ResolveAsync(true);

        public void Init(IContainer currentLocation)
        {
            _currentLocation.OnNext(currentLocation);
        }

        private static IAbsolutePath? GetSelectedItemByItems(IEnumerable<IItem> items)
        {
            //TODO: 
            return new AbsolutePath(items.First());
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