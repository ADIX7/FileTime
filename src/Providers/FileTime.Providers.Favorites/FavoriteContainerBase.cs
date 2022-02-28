using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Providers.Favorites.Persistence;

namespace FileTime.Providers.Favorites
{
    public abstract class FavoriteContainerBase : IContainer
    {
        private readonly List<FavoriteContainer> _containers;
        private List<IItem> _items;
        private readonly List<FavoriteElement> _elements;
        private readonly List<Exception> _exceptions = new();
        private readonly PersistenceService _persistenceService;

        public bool IsExists => true;

        public IReadOnlyList<Exception> Exceptions { get; }

        public bool AllowRecursiveDeletion => false;

        public bool Loading => false;

        public bool CanHandleEscape => false;

        public bool IsLoaded => true;

        public bool SupportsDirectoryLevelSoftDelete => false;

        public AsyncEventHandler Refreshed { get; } = new();

        public AsyncEventHandler<bool> LoadingChanged { get; } = new();

        public string Name { get; }

        public string DisplayName { get; }

        public string? FullName { get; }

        public string? NativePath { get; }

        public bool IsHidden => false;

        public bool IsDestroyed => false;

        public SupportsDelete CanDelete => SupportsDelete.True;

        public bool CanRename => true;

        public FavoriteContentProvider Provider { get; }

        IContentProvider IItem.Provider => Provider;

        protected IContainer? Parent { get; set; }

        public IReadOnlyList<FavoriteContainer> Containers { get; }
        public IReadOnlyList<FavoriteElement> Elements { get; }

        protected FavoriteContainerBase(PersistenceService persistenceService, FavoriteContentProvider provider, IContainer parent, string name)
            : this(persistenceService, name, parent.FullName == null ? name : parent.FullName + Constants.SeparatorChar + name)
        {
            Provider = provider;
            Parent = parent;
        }

        protected FavoriteContainerBase(PersistenceService persistenceService, string name)
            : this(persistenceService, name, null)
        {
            Provider = (FavoriteContentProvider)this;
        }

        private FavoriteContainerBase(PersistenceService persistenceService, string name, string? fullName)
        {
            _containers = new List<FavoriteContainer>();
            _items = new List<IItem>();
            _elements = new List<FavoriteElement>();
            _persistenceService = persistenceService;

            Containers = _containers.AsReadOnly();
            Elements = _elements.AsReadOnly();

            Exceptions = _exceptions.AsReadOnly();
            DisplayName = Name = name;
            NativePath = FullName = fullName;
            Provider = null!;
        }

        public Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

        public async Task<IContainer> CreateContainerAsync(string name)
        {
            var container = new FavoriteContainer(_persistenceService, Provider, this, name);
            await AddContainerAsync(container);
            return container;
        }

        public Task<IElement> CreateElementAsync(string name) => throw new NotSupportedException();

        public Task Delete(bool hardDelete = false)
        {
            throw new NotImplementedException();
        }

        public async Task RefreshAsync(CancellationToken token = default)
        {
            if (Refreshed != null) await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        }

        public Task Rename(string newName)
        {
            throw new NotImplementedException();
        }

        protected async Task SaveFavoritesAsync()
        {
            await _persistenceService.SaveFavorites(Provider.Containers.Cast<IFavoriteItem>().Concat(Provider.Elements.Cast<IFavoriteItem>()));
        }

        public async Task AddContainerAsync(FavoriteContainer container)
        {
            _containers.Add(container);
            UpdateItems();
            await SaveFavoritesAsync();
            await RefreshAsync();
        }

        public async Task AddContainersAsync(IEnumerable<FavoriteContainer> containers)
        {
            _containers.AddRange(containers);
            UpdateItems();
            await SaveFavoritesAsync();
            await RefreshAsync();
        }

        public async Task DeleteContainerAsync(FavoriteContainer container)
        {
            _containers.Remove(container);
            UpdateItems();
            await RefreshAsync();
        }

        public async Task AddElementAsync(FavoriteElement element)
        {
            _elements.Add(element);
            UpdateItems();
            await SaveFavoritesAsync();
            await RefreshAsync();
        }

        public async Task AddElementsAsync(IEnumerable<FavoriteElement> elements)
        {
            _elements.AddRange(elements);
            UpdateItems();
            await SaveFavoritesAsync();
            await RefreshAsync();
        }

        public async Task DeleteElementAsync(FavoriteElement element)
        {
            _elements.Remove(element);
            UpdateItems();
            await RefreshAsync();
        }

        private void UpdateItems()
        {
            _items = _containers.Cast<IItem>().Concat(_elements).ToList();
        }

        public async Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            await InitIfNeeded();
            return _containers;
        }

        public async Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default)
        {
            await InitIfNeeded();
            return _elements;
        }

        public async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            await InitIfNeeded();
            return _items;
        }

        public virtual Task InitIfNeeded() => Task.CompletedTask;

        public async Task<bool> IsExistsAsync(string name)
        {
            var items = await GetItems();
            return items?.Any(i => i.Name == name) ?? false;
        }

        public virtual Task<bool> CanOpenAsync() => Task.FromResult(_exceptions.Count == 0);

        public void Unload() { }

        public Task<ContainerEscapeResult> HandleEscape() => throw new NotSupportedException();

        public async Task RunWithLoading(Func<CancellationToken, Task> func, CancellationToken token = default)
        {
            await func(token);
        }

        public void Destroy() { }

        public IContainer? GetParent() => Parent;
    }
}