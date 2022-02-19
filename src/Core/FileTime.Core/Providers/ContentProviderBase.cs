using AsyncEvent;
using FileTime.Core.Models;

namespace FileTime.Core.Providers
{
    public abstract class ContentProviderBase<T> : AbstractContainer<T>, IContentProvider
        where T : class, IContentProvider
    {
        private readonly object _initializationGuard = new();
        private bool _initialized;
        private bool _initializing;
        private IContainer? _parent;
        protected IReadOnlyList<IContainer>? RootContainers { get; private set; }
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
                (await GetContainers())?.Append(newRootContainer).OrderBy(c => c.Name).ToList().AsReadOnly()
                ?? new List<IContainer>() { newRootContainer }.AsReadOnly();
        }

        protected async Task AddRootContainers(IEnumerable<IContainer> newRootContainers)
        {
            RootContainers =
                (await GetContainers())?.Concat(newRootContainers).OrderBy(c => c.Name).ToList().AsReadOnly()
                ?? new List<IContainer>(newRootContainers).AsReadOnly();
        }

        protected void SetRootContainers(IEnumerable<IContainer> newRootContainers)
            => RootContainers = newRootContainers.OrderBy(c => c.Name).ToList().AsReadOnly();

        protected void ClearRootContainers() => RootContainers = new List<IContainer>().AsReadOnly();

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
                _initialized = true;
                await Init();
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