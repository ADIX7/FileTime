using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.Timeline
{
    public class TimeContainer : IContainer
    {
        private readonly IContainer? _parent;
        private readonly PointInTime _pointInTime;

        public bool IsLoaded => true;

        public AsyncEventHandler Refreshed { get; } = new AsyncEventHandler();

        public string Name { get; }

        public string? FullName { get; }

        public bool IsHidden => false;

        public bool CanDelete => true;

        public bool CanRename => true;

        public IContentProvider Provider { get; }
        public IContentProvider VirtualProvider { get; }

        public TimeContainer(string name, IContainer parent, IContentProvider contentProvider, IContentProvider virtualContentProvider, PointInTime pointInTime)
        {
            _parent = parent;
            _pointInTime = pointInTime;

            Name = name;
            Provider = contentProvider;
            VirtualProvider = virtualContentProvider;
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
        }

        public async Task<IContainer> Clone() => new TimeContainer(Name, await _parent!.Clone(), Provider, VirtualProvider, _pointInTime);

        public Task<IContainer> CreateContainer(string name) => Task.FromResult((IContainer)new TimeContainer(name, this, Provider, VirtualProvider, _pointInTime));

        public Task<IElement> CreateElement(string name) => Task.FromResult((IElement)new TimeElement(name, this, Provider, VirtualProvider));

        public Task Delete() => Task.CompletedTask;

        public async Task<IItem?> GetByPath(string path)
        {
            var paths = path.Split(Constants.SeparatorChar);

            var item = (await GetItems())!.FirstOrDefault(i => i.Name == paths[0]);

            if (paths.Length == 1)
            {
                return item;
            }

            if (item is IContainer container)
            {
                return await container.GetByPath(string.Join(Constants.SeparatorChar, paths.Skip(1)));
            }

            return null;
        }

        public Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default) =>
            Task.FromResult(
                (IReadOnlyList<IContainer>?)_pointInTime
                    .Differences
                    .Where(d =>
                        d.Type == DifferenceItemType.Container
                        && GetParentPath(d.AbsolutePath.Path) == FullName)
                    .Select(MapContainer)
                    .ToList()
                    .AsReadOnly()
                );

        public Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default) =>
            Task.FromResult(
                (IReadOnlyList<IElement>?)_pointInTime
                    .Differences
                    .Where(d =>
                        d.Type == DifferenceItemType.Element
                        && GetParentPath(d.AbsolutePath.Path) == FullName)
                    .Select(MapElement)
                    .ToList()
                    .AsReadOnly()
                );

        public async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            var containers = (await GetContainers(token))!;
            var elements = (await GetElements(token))!;

            return containers.Cast<IItem>().Concat(elements).ToList().AsReadOnly();
        }

        public IContainer? GetParent() => _parent;

        public async Task<bool> IsExists(string name) => (await GetItems())?.Any(i => i.Name == name) ?? false;

        public async Task Refresh() => await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty);

        public Task Rename(string newName) => Task.CompletedTask;

        private static string GetParentPath(string path) => string.Join(Constants.SeparatorChar, path.Split(Constants.SeparatorChar).Take(-1));

        private IContainer MapContainer(Difference containerDiff)
        {
            if (containerDiff.Type != DifferenceItemType.Container) throw new ArgumentException($"{nameof(containerDiff)}'s {nameof(Difference.Type)} property is not {DifferenceItemType.Container}.");
            return new TimeContainer(containerDiff.Name, this, Provider, containerDiff.AbsolutePath.VirtualContentProvider ?? containerDiff.AbsolutePath.ContentProvider, _pointInTime);
        }

        private IElement MapElement(Difference elementDiff)
        {
            if (elementDiff.Type != DifferenceItemType.Container) throw new ArgumentException($"{elementDiff}'s {nameof(Difference.Type)} property is not {DifferenceItemType.Element}.");
            return new TimeElement(elementDiff.Name, this, Provider, elementDiff.AbsolutePath.VirtualContentProvider ?? elementDiff.AbsolutePath.ContentProvider);
        }
    }
}