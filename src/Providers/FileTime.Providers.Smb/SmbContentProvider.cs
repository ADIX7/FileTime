using System;
using AsyncEvent;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Providers.Smb
{
    public class SmbContentProvider : IContentProvider
    {
        private IContainer? _parent;
        private readonly IInputInterface _inputInterface;
        private readonly List<IContainer> _rootContainers;
        private readonly IReadOnlyList<IContainer> _rootContainersReadOnly;
        private IReadOnlyList<IItem>? _items;
        private readonly IReadOnlyList<IElement>? _elements = new List<IElement>().AsReadOnly();

        public string Name { get; } = "smb";

        public string? FullName { get; }

        public bool IsHidden => false;
        public bool IsLoaded => true;

        public IContentProvider Provider => this;
        public SupportsDelete CanDelete => SupportsDelete.False;
        public bool CanRename => false;
        public IReadOnlyList<Exception> Exceptions { get; } = new List<Exception>().AsReadOnly();

        public AsyncEventHandler Refreshed { get; } = new();

        public bool SupportsDirectoryLevelSoftDelete => false;

        public SmbContentProvider(IInputInterface inputInterface)
        {
            _rootContainers = new List<IContainer>();
            _items = new List<IItem>();
            _rootContainersReadOnly = _rootContainers.AsReadOnly();
            _inputInterface = inputInterface;
        }

        public async Task<IContainer> CreateContainer(string name)
        {
            var fullName = "\\\\" + name;
            var container = _rootContainers.Find(c => c.Name == name);

            if (container == null)
            {
                container = new SmbServer(fullName, this, _inputInterface);
                _rootContainers.Add(container);
                _items = _rootContainers.OrderBy(c => c.Name).ToList().AsReadOnly();
            }

            await Refresh();

            return container;
        }

        public Task<IElement> CreateElement(string name)
        {
            throw new NotSupportedException();
        }

        public Task Delete(bool hardDelete = false)
        {
            throw new NotSupportedException();
        }

        public async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            if (path == null) return this;

            var pathParts = path.TrimStart(Constants.SeparatorChar).Split(Constants.SeparatorChar);

            var rootContainer = _rootContainers.Find(c => c.Name == pathParts[0]);

            if (rootContainer == null)
            {
                return null;
            }

            var remainingPath = string.Join(Constants.SeparatorChar, pathParts.Skip(1));
            return remainingPath.Length == 0 ? rootContainer : await rootContainer.GetByPath(remainingPath, acceptDeepestMatch);
        }

        public IContainer? GetParent() => _parent;

        public Task<IContainer> Clone() => Task.FromResult((IContainer)this);

        public async Task<bool> IsExists(string name) => (await GetItems())?.Any(i => i.Name == name) ?? false;

        public async Task Refresh() => await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty);

        public bool CanHandlePath(string path) => path.StartsWith("smb://") || path.StartsWith(@"\\");

        public void SetParent(IContainer container) => _parent = container;
        public Task<IReadOnlyList<IContainer>> GetRootContainers(CancellationToken token = default) => Task.FromResult(_rootContainersReadOnly);

        public Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default) => Task.FromResult(_items);
        public Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default) => Task.FromResult((IReadOnlyList<IContainer>?)_rootContainersReadOnly);
        public Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default) => Task.FromResult(_elements);

        public Task Rename(string newName) => throw new NotSupportedException();
        public Task<bool> CanOpen() => Task.FromResult(true);
    }
}