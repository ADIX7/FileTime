using AsyncEvent;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Sftp
{
    public class SftpContentProvider : IContentProvider
    {
        private IContainer? _parent;
        private readonly IInputInterface _inputInterface;
        private readonly List<IContainer> _rootContainers;
        private readonly IReadOnlyList<IContainer> _rootContainersReadOnly;
        private IReadOnlyList<IItem>? _items;
        private readonly IReadOnlyList<IElement> _elements = new List<IElement>().AsReadOnly();
        private readonly ILogger<SftpContentProvider> _logger;

        public bool SupportsContentStreams => false;

        public IReadOnlyList<Exception> Exceptions { get; } = new List<Exception>().AsReadOnly();

        public bool IsLoaded => true;

        public bool SupportsDirectoryLevelSoftDelete => false;

        public AsyncEventHandler Refreshed { get; } = new AsyncEventHandler();

        public string Name => "sftp";

        public string? FullName => null;

        public string? NativePath => null;

        public bool IsHidden => false;

        public bool IsDestroyed => false;

        public SupportsDelete CanDelete => SupportsDelete.False;

        public bool CanRename => false;

        public IContentProvider Provider => this;

        public string Protocol => "sftp://";

        public SftpContentProvider(IInputInterface inputInterface, ILogger<SftpContentProvider> logger)
        {
            _logger = logger;
            _rootContainers = new List<IContainer>();
            _items = new List<IItem>();
            _rootContainersReadOnly = _rootContainers.AsReadOnly();
            _inputInterface = inputInterface;
        }

        public bool CanHandlePath(string path) => path.StartsWith("sftp://");
        public Task<bool> CanOpenAsync() => Task.FromResult(true);
        public Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

        public async Task<IContainer> CreateContainerAsync(string name)
        {
            var container = _rootContainers.Find(c => c.Name == name);

            if (container == null)
            {
                container = new SftpServer(name, this, _inputInterface);
                _rootContainers.Add(container);
                _items = _rootContainers.OrderBy(c => c.Name).ToList().AsReadOnly();
            }

            await RefreshAsync();

            //await SaveServers();

            return container;
        }

        public Task<IElement> CreateElementAsync(string name)
        {
            throw new NotSupportedException();
        }

        public Task Delete(bool hardDelete = false)
        {
            throw new NotSupportedException();
        }

        public void Destroy() { }

        public async Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            await Init();
            return _rootContainersReadOnly;
        }
        public Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default) => Task.FromResult((IReadOnlyList<IElement>?)_elements);

        public async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            await Init();
            return _items;
        }

        public IContainer? GetParent() => _parent;
        public Task<IReadOnlyList<IContainer>> GetRootContainers(CancellationToken token = default) => Task.FromResult(_rootContainersReadOnly);

        public async Task<bool> IsExistsAsync(string name) => (await GetItems())?.Any(i => i.Name == name) ?? false;

        public async Task RefreshAsync(CancellationToken token = default) => await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);

        public Task Rename(string newName) => throw new NotSupportedException();

        public void SetParent(IContainer container) => _parent = container;

        public void Unload() { }

        private Task Init()
        {
            return Task.CompletedTask;
        }

        public async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            if (path == null) return this;

            var pathParts = path.TrimStart(Constants.SeparatorChar).Split(Constants.SeparatorChar);

            var rootContainer = (await GetContainers())?.FirstOrDefault(c => c.Name == pathParts[0]);

            if (rootContainer == null)
            {
                return null;
            }

            var remainingPath = string.Join(Constants.SeparatorChar, pathParts.Skip(1));
            try
            {
                return remainingPath.Length == 0 ? rootContainer : await rootContainer.GetByPath(remainingPath, acceptDeepestMatch);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting path {Path}", path);
                if (acceptDeepestMatch)
                {
                    return rootContainer ?? this;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}