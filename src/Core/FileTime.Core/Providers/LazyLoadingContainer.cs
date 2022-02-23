using AsyncEvent;
using FileTime.Core.Models;

namespace FileTime.Core.Providers
{
    public abstract class LazyLoadingContainer<TProvider, TContainer, TElement> : AbstractContainer<TProvider>
        where TProvider : class, IContentProvider
        where TContainer : class, IContainer
        where TElement : class, IElement
    {
        protected List<TContainer> Containers { get; }
        private IReadOnlyList<IItem> _items;
        protected List<TElement> Elements { get; }

        private readonly IReadOnlyList<IContainer> _containersReadOnly;
        private readonly IReadOnlyList<IElement> _elementsReadOnly;

        protected LazyLoadingContainer(TProvider provider, IContainer parent, string name) : base(provider, parent, name)
        {
            Containers = new List<TContainer>();
            Elements = new List<TElement>();

            _containersReadOnly = Containers.AsReadOnly();
            _elementsReadOnly = Elements.AsReadOnly();
            _items = Containers.Cast<IItem>().Concat(Elements).ToList().AsReadOnly();
        }

        public async Task RunWithLazyLoading(Func<CancellationToken, Task> func, CancellationToken token = default)
        {
            try
            {
                LazyLoading = true;
                await LazyLoadingChanged.InvokeAsync(this, LazyLoading, token);
                await func(token);
            }
            finally
            {
                LazyLoading = false;
                await LazyLoadingChanged.InvokeAsync(this, LazyLoading, token);
            }
        }

        public virtual async Task AddContainerAsync(TContainer container)
        {
            Containers.Add(container);
            await UpdateChildren();
        }

        public virtual async Task AddElementAsync(TElement element)
        {
            Elements.Add(element);
            await UpdateChildren();
        }

        private async Task UpdateChildren()
        {
            _items = Containers.Cast<IItem>().Concat(Elements).ToList().AsReadOnly();
            await RefreshAsync();
        }

        public override async Task RefreshAsync(CancellationToken token = default)
        {
            if (Refreshed != null) await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        }

        public override Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default) => Task.FromResult(Enumerable.Empty<IItem>());

        public override Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default) => Task.FromResult((IReadOnlyList<IContainer>?)_containersReadOnly);

        public override Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default) => Task.FromResult((IReadOnlyList<IElement>?)_elementsReadOnly);

        public override Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default) => Task.FromResult((IReadOnlyList<IItem>?)_items);
    }
}