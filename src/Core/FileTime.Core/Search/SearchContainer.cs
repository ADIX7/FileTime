using System.Threading.Tasks;
using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.Search
{
    public class SearchContainer : AbstractContainer<IContentProvider>
    {
        private int _childContainerCounter;
        private int _childElementCounter;
        private readonly List<IContainer> _containers;
        private IReadOnlyList<IItem> _items;
        private readonly List<IElement> _elements;

        private readonly IReadOnlyList<IContainer> _containersReadOnly;
        private readonly IReadOnlyList<IElement> _elementsReadOnly;
        public SearchTaskBase SearchTaskBase { get; }

        public IContainer SearchBaseContainer { get; }
        public override bool IsExists => throw new NotImplementedException();

        public SearchContainer(IContainer searchBaseContainer, SearchTaskBase searchTaskBase) : base(searchBaseContainer.Provider, searchBaseContainer.GetParent()!, searchBaseContainer.Name)
        {
            SearchBaseContainer = searchBaseContainer;
            _containers = new List<IContainer>();
            _elements = new List<IElement>();
            SearchTaskBase = searchTaskBase;

            _containersReadOnly = _containers.AsReadOnly();
            _elementsReadOnly = _elements.AsReadOnly();
            _items = _containers.Cast<IItem>().Concat(_elements).ToList().AsReadOnly();

            UseLazyLoad = true;
            CanHandleEscape = true;
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

        public async Task AddContainer(IContainer container)
        {
            var childContainer = new ChildSearchContainer(this, Provider, container, "container" + _childContainerCounter++, container.DisplayName, SearchTaskBase.GetDisplayName(container));
            _containers.Add(childContainer);
            await UpdateChildren();
        }

        public async Task AddElement(IElement element)
        {
            var childElement = new ChildSearchElement(this, Provider, element.GetParent()!, element, "element" + _childElementCounter++, SearchTaskBase.GetDisplayName(element));
            _elements.Add(childElement);
            await UpdateChildren();
        }

        private async Task UpdateChildren()
        {
            _items = _containers.Cast<IItem>().Concat(_elements).ToList().AsReadOnly();
            await RefreshAsync();
        }

        public override async Task RefreshAsync(CancellationToken token = default)
        {
            if (Refreshed != null) await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        }

        public override Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default) => Task.FromResult((IReadOnlyList<IContainer>?)_containersReadOnly);

        public override Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default) => Task.FromResult((IReadOnlyList<IElement>?)_elementsReadOnly);

        public override Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default) => Task.FromResult((IReadOnlyList<IItem>?)_items);

        public override Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

        public override Task<IContainer> CreateContainerAsync(string name) => throw new NotSupportedException();

        public override Task<IElement> CreateElementAsync(string name) => throw new NotSupportedException();

        public override Task Delete(bool hardDelete = false) => throw new NotSupportedException();

        public override Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default) => throw new NotImplementedException();

        public override Task Rename(string newName) => throw new NotSupportedException();

        public override Task<ContainerEscapeResult> HandleEscape()
        {
            if (SearchTaskBase.Cancel()) return Task.FromResult(new ContainerEscapeResult(true));

            return Task.FromResult(new ContainerEscapeResult(SearchBaseContainer));
        }
    }
}