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

        public SupportsDelete CanDelete => SupportsDelete.True;

        public bool CanRename => true;

        public IContentProvider Provider { get; }
        public IContentProvider VirtualProvider { get; }
        public IReadOnlyList<Exception> Exceptions { get; } = new List<Exception>().AsReadOnly();

        public bool SupportsDirectoryLevelSoftDelete => false;

        public bool IsDestroyed { get; private set; }

        //FIXME: currently this can be different of the real items NativePath, should be fixed
        public string? NativePath => FullName;
        public bool IsExists => true;

        public TimeContainer(string name, IContainer parent, IContentProvider contentProvider, IContentProvider virtualContentProvider, PointInTime pointInTime)
        {
            _parent = parent;
            _pointInTime = pointInTime;

            Name = name;
            Provider = contentProvider;
            VirtualProvider = virtualContentProvider;
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
        }

        public async Task<IContainer> CloneAsync() => new TimeContainer(Name, await _parent!.CloneAsync(), Provider, VirtualProvider, _pointInTime);

        public Task<IContainer> CreateContainerAsync(string name) => Task.FromResult((IContainer)new TimeContainer(name, this, Provider, VirtualProvider, _pointInTime));

        public Task<IElement> CreateElementAsync(string name) => Task.FromResult((IElement)new TimeElement(name, this, Provider, VirtualProvider));

        public Task Delete(bool hardDelete = false) => Task.CompletedTask;

        public async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            var paths = path.Split(Constants.SeparatorChar);

            var item = (await GetItems())!.FirstOrDefault(i => i.Name == paths[0]);

            if (paths.Length == 1)
            {
                return item;
            }

            if (item is IContainer container)
            {
                return await container.GetByPath(string.Join(Constants.SeparatorChar, paths.Skip(1)), acceptDeepestMatch);
            }

            return null;
        }

        public Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default) =>
            Task.FromResult(
                (IReadOnlyList<IContainer>?)_pointInTime
                    .Differences
                    .Where(d =>
                        d.AbsolutePath.Type == AbsolutePathType.Container
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
                        d.AbsolutePath.Type == AbsolutePathType.Element
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

        public async Task<bool> IsExistsAsync(string name) => (await GetItems())?.Any(i => i.Name == name) ?? false;

        public async Task RefreshAsync(CancellationToken token = default) => await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);

        public Task Rename(string newName) => Task.CompletedTask;

        private static string GetParentPath(string path) => string.Join(Constants.SeparatorChar, path.Split(Constants.SeparatorChar).Take(-1));

        private IContainer MapContainer(Difference containerDiff)
        {
            if (containerDiff.AbsolutePath.Type != AbsolutePathType.Container) throw new ArgumentException($"{nameof(containerDiff)}'s {nameof(AbsolutePath.Type)} property is not {AbsolutePathType.Container}.");
            return new TimeContainer(containerDiff.Name, this, Provider, containerDiff.AbsolutePath.VirtualContentProvider ?? containerDiff.AbsolutePath.ContentProvider, _pointInTime);
        }

        private IElement MapElement(Difference elementDiff)
        {
            if (elementDiff.AbsolutePath.Type != AbsolutePathType.Element) throw new ArgumentException($"{elementDiff}'s {nameof(AbsolutePath.Type)} property is not {AbsolutePathType.Element}.");
            return new TimeElement(elementDiff.Name, this, Provider, elementDiff.AbsolutePath.VirtualContentProvider ?? elementDiff.AbsolutePath.ContentProvider);
        }
        public Task<bool> CanOpenAsync() => Task.FromResult(true);

        public void Destroy() => IsDestroyed = true;
        public void Unload() { }
    }
}