using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Core.Providers.ContainerProperty;

namespace FileTime.Core.ContainerSizeScanner
{
    public class ContainerSizeContainer : LazyLoadingContainer<ContainerScanSnapshotProvider, ContainerSizeContainer, ContainerSizeElement>, IItemWithSize, IHaveCreatedAt, IHaveAttributes
    {
        public override bool IsExists => true;

        public long? Size { get; private set; }
        public AsyncEventHandler<long?> SizeChanged { get; } = new();

        public bool AllowSizeScan { get; private set; } = true;

        private readonly IContainer _baseContainer;

        public string Attributes => _baseContainer is IHaveAttributes haveAttributes ? haveAttributes.Attributes : "";

        public DateTime? CreatedAt => _baseContainer is IHaveCreatedAt haveCreatedAt ? haveCreatedAt.CreatedAt : null;

        public ContainerSizeContainer(ContainerScanSnapshotProvider provider, IContainer parent, IContainer baseContainer, string? displayName = null) : base(provider, parent, baseContainer.Name)
        {
            _baseContainer = baseContainer;
            CanDelete = SupportsDelete.True;
            AllowRecursiveDeletion = false;
            CanHandleEscape = true;
            if (displayName != null)
            {
                DisplayName = displayName;
            }
        }

        public override Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

        public override Task<IContainer> CreateContainerAsync(string name) => throw new NotSupportedException();

        public override Task<IElement> CreateElementAsync(string name) => throw new NotSupportedException();

        public override async Task Delete(bool hardDelete = false)
        {
            if (GetParent() is ContainerScanSnapshotProvider provider)
            {
                await provider.RemoveSnapshotAsync(this);
            }
        }

        public override async Task RefreshAsync(CancellationToken token = default)
        {
            if (Refreshed != null) await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        }

        public override Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default) => throw new NotImplementedException();

        public override Task Rename(string newName) => throw new NotSupportedException();

        public override async Task AddContainerAsync(ContainerSizeContainer container)
        {
            await base.AddContainerAsync(container);
            container.SizeChanged.Add(ChildContainerSizeChanged);
        }

        public override async Task AddElementAsync(ContainerSizeElement element)
        {
            await base.AddElementAsync(element);
        }

        public IEnumerable<IItemWithSize> GetItemsWithSize() => Containers.Cast<IItemWithSize>().Concat(Elements);

        private async Task ChildContainerSizeChanged(object? sender, long? size, CancellationToken token) => await UpdateSize();

        public async Task UpdateSize()
        {
            Size = Containers.Aggregate(0L, (sum, c) => sum + c.Size ?? 0)
                    + Elements.Aggregate(0L, (sum, e) => sum + e.Size);

            await SizeChanged.InvokeAsync(this, Size);
        }

        public override Task<ContainerEscapeResult> HandleEscape()
        {
            if (AllowSizeScan)
            {
                AllowSizeScan = false;
                return Task.FromResult(new ContainerEscapeResult(true));
            }

            return Task.FromResult(new ContainerEscapeResult(_baseContainer));
        }
    }
}