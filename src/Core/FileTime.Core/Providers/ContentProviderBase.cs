using AsyncEvent;
using FileTime.Core.Models;

namespace FileTime.Core.Providers
{
    public abstract class ContentProviderBase<T> : AbstractContainer<T>, IContentProvider
        where T : class, IContentProvider
    {
        private readonly object _initializationGuard = new();
        protected IReadOnlyList<IContainer> _rootContainers;
        private bool _initialized;
        private bool _initializing;
        private IContainer? _parent;
        protected List<IContainer>? RootContainers { get; private set; }
        public override bool IsExists => true;

        protected ContentProviderBase(
            string name,
            string? fullName,
            string protocol,
            bool supportsContentStreams)
        : base(name, fullName)
        {
            Protocol = protocol;
            SupportsContentStreams = supportsContentStreams;
            RootContainers = new List<IContainer>();
            _rootContainers = RootContainers.AsReadOnly();
            AllowRecursiveDeletion = false;

            CanRename = false;
            CanDelete = SupportsDelete.False;
        }

        public virtual bool SupportsContentStreams { get; }

        public virtual string Protocol { get; }

        public abstract Task<bool> CanHandlePath(string path);
        public override IContainer? GetParent() => _parent;

        public void SetParent(IContainer parent) => _parent = parent;

        protected async Task AddRootContainer(IContainer newRootContainer)
        {
            RootContainers =
                (await GetContainers())?.Append(newRootContainer).OrderBy(c => c.Name).ToList()
                ?? new List<IContainer>() { newRootContainer };

            _rootContainers = RootContainers.AsReadOnly();
            await RefreshAsync();
        }

        protected async Task AddRootContainers(IEnumerable<IContainer> newRootContainers)
        {
            RootContainers =
                (await GetContainers())?.Concat(newRootContainers).OrderBy(c => c.Name).ToList()
                ?? new List<IContainer>(newRootContainers);

            _rootContainers = RootContainers.AsReadOnly();
            await RefreshAsync();
        }

        protected async Task SetRootContainers(IEnumerable<IContainer> newRootContainers)
        {
            RootContainers = newRootContainers.OrderBy(c => c.Name).ToList();
            _rootContainers = RootContainers.AsReadOnly();
            await RefreshAsync();
        }

        protected void ClearRootContainers()
        {
            RootContainers = new List<IContainer>();
            _rootContainers = RootContainers.AsReadOnly();
        }

        public override async Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            await InitIfNeeded();
            return RootContainers;
        }

        public override async Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default)
        {
            await InitIfNeeded();
            return new List<IElement>();
        }

        public override async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            await InitIfNeeded();
            return RootContainers;
        }

        private async Task InitIfNeeded()
        {
            while (true)
            {
                lock (_initializationGuard)
                {
                    if (_initialized) return;
                    if (!_initializing)
                    {
                        _initializing = true;
                        break;
                    }
                }
                await Task.Delay(1);
            }

            try
            {
                if (_initialized) return;
                await Init();
                _initialized = true;
                IsLoaded = true;
            }
            finally
            {
                lock (_initializationGuard)
                {
                    _initializing = false;
                }
            }
        }

        protected virtual Task Init()
        {
            if (RootContainers == null) ClearRootContainers();
            return Task.CompletedTask;
        }

        public override Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default) { throw new NotImplementedException($"{nameof(RefreshItems)} should not be called in {nameof(ContentProviderBase<T>)}."); }

        public override Task<bool> IsExistsAsync(string name) => Task.FromResult(RootContainers?.Any(i => i.Name == name) ?? false);

        public override async Task RefreshAsync(CancellationToken token = default) => await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        public override Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);
        public override Task Delete(bool hardDelete = false) => throw new NotSupportedException();
        public override Task Rename(string newName) => throw new NotSupportedException();
        public override Task<bool> CanOpenAsync() => Task.FromResult(true);

        public override void Unload() { }

        public override void Destroy() { }
    }
}