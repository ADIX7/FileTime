using AsyncEvent;
using FileTime.Core.Models;

namespace FileTime.Core.Providers
{
    public abstract class AbstractContainer<TProvider> : IContainer where TProvider : IContentProvider
    {
        private readonly IContainer _parent;
        private readonly List<Exception> _exceptions = new();
        private IReadOnlyList<IContainer>? _containers;
        private IReadOnlyList<IItem>? _items;
        private IReadOnlyList<IElement>? _elements;

        public IReadOnlyList<Exception> Exceptions { get; }

        public bool IsLoaded { get; protected set; }

        public bool SupportsDirectoryLevelSoftDelete { get; protected set; }

        public AsyncEventHandler Refreshed { get; protected set; } = new();

        public string Name { get; protected set; }

        public string? FullName { get; protected set; }

        public string? NativePath { get; protected set; }

        public virtual bool IsHidden { get; protected set; }

        public bool IsDestroyed { get; protected set; }

        public virtual SupportsDelete CanDelete { get; protected set; }

        public virtual bool CanRename { get; protected set; }

        public TProvider Provider { get; }

        IContentProvider IItem.Provider => Provider;

        public abstract bool IsExists { get; }

        protected AbstractContainer(TProvider provider, IContainer parent, string name)
        {
            _parent = parent;
            Provider = provider;
            Name = name;
            FullName = parent.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
            Exceptions = _exceptions.AsReadOnly();
        }

        public virtual Task<bool> CanOpenAsync() => Task.FromResult(_exceptions.Count == 0);

        public abstract Task<IContainer> CloneAsync();

        public abstract Task<IContainer> CreateContainerAsync(string name);

        public abstract Task<IElement> CreateElementAsync(string name);

        public abstract Task Delete(bool hardDelete = false);

        public virtual void Destroy()
        {
            _items = null;
            _containers = null;
            _elements = null;
            IsLoaded = false;
            IsDestroyed = true;
            Refreshed = new AsyncEventHandler();
        }

        public virtual async Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            if (_containers == null) await RefreshAsync(token);
            return _containers;
        }

        public virtual async Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default)
        {
            if (_elements == null) await RefreshAsync(token);
            return _elements;
        }

        public virtual async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            if (_items == null) await RefreshAsync(token);
            return _items;
        }

        public virtual IContainer? GetParent() => _parent;

        public virtual async Task<bool> IsExistsAsync(string name)
        {
            var items = await GetItems();
            return items?.Any(i => i.Name == name) ?? false;
        }

        public virtual async Task RefreshAsync(CancellationToken token = default)
        {
            _exceptions.Clear();
            var containers = new List<IContainer>();
            var elements = new List<IElement>();
            foreach (var item in await RefreshItems(token))
            {
                if (item is IContainer container)
                {
                    containers.Add(container);
                }
                else if (item is IElement element)
                {
                    elements.Add(element);
                }
            }

            if (_items != null)
            {
                foreach (var item in _items)
                {
                    item.Destroy();
                }
            }

            _containers = containers.OrderBy(c => c.Name).ToList().AsReadOnly();
            _elements = elements.OrderBy(e => e.Name).ToList().AsReadOnly();
            _items = _containers.Cast<IItem>().Concat(_elements).ToList().AsReadOnly();
            IsLoaded = true;
            if (Refreshed != null) await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        }

        public abstract Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default);

        public abstract Task Rename(string newName);

        public virtual void Unload()
        {
            _items = null;
            _containers = null;
            _elements = null;
            IsLoaded = false;
        }

        protected void AddException(Exception e) => _exceptions.Add(e);
    }
}