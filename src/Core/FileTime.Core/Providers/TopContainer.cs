using AsyncEvent;
using FileTime.Core.Models;

namespace FileTime.Core.Providers
{
    public class TopContainer : IContainer
    {
        private readonly List<IContentProvider> _contentProviders;
        private readonly IReadOnlyList<IContainer>? _containers;
        private readonly IReadOnlyList<IItem>? _items;
        private readonly IReadOnlyList<IElement>? _elements = new List<IElement>().AsReadOnly();

#pragma warning disable CS8603 // Possible null reference return.
        public string Name => null;
        public string DisplayName => null;
#pragma warning restore CS8603 // Possible null reference return.

        public string? FullName => null;
        public string? NativePath => null;

        public bool IsHidden => false;
        public bool IsLoaded => true;

#pragma warning disable CS8603 // Possible null reference return.
        public IContentProvider Provider => null;
#pragma warning restore CS8603 // Possible null reference return.
        public SupportsDelete CanDelete => SupportsDelete.False;
        public bool CanRename => false;

        public AsyncEventHandler Refreshed { get; } = new();

        public IReadOnlyList<Exception> Exceptions { get; } = new List<Exception>().AsReadOnly();

        public bool SupportsDirectoryLevelSoftDelete => false;

        public bool IsDestroyed => false;
        public bool IsExists => true;
        public bool AllowRecursiveDeletion => false;
        public bool Loading => false;
        public bool CanHandleEscape => false;
        public AsyncEventHandler<bool> LoadingChanged { get; protected set; } = new();

        public TopContainer(IEnumerable<IContentProvider> contentProviders)
        {
            _contentProviders = new List<IContentProvider>(contentProviders);
            _containers = _contentProviders.AsReadOnly();
            _items = _containers.Cast<IItem>().ToList().AsReadOnly();

            foreach (var contentProvider in contentProviders)
            {
                contentProvider.SetParent(this);
            }
        }

        public Task<IContainer> CreateContainerAsync(string name) => throw new NotImplementedException();

        public Task<IElement> CreateElementAsync(string name) => throw new NotImplementedException();

        public Task Delete(bool hardDelete = false) => throw new NotImplementedException();

        public Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false) => throw new NotImplementedException();

        public IContainer? GetParent() => null;

        public Task<bool> IsExistsAsync(string name) => throw new NotImplementedException();

        public async Task RefreshAsync(CancellationToken token = default) => await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);

        public Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default) => Task.FromResult(_items);
        public Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default) => Task.FromResult(_containers);
        public Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default) => Task.FromResult(_elements);

        public Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

        public Task Rename(string newName) => throw new NotSupportedException();

        public Task<bool> CanOpenAsync() => Task.FromResult(true);

        public void Destroy() { }

        public void Unload() { }
        public Task<ContainerEscapeResult> HandleEscape() => Task.FromResult(new ContainerEscapeResult(false));

        public async Task RunWithLoading(Func<CancellationToken, Task> func, CancellationToken token = default)
        {
            await func(token);
        }
    }
}