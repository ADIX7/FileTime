using FileTime.Core.Models;

namespace FileTime.Core.Search
{
    public abstract class SearchTaskBase
    {
        private readonly object _searchGuard = new();
        private readonly IContainer _baseContainer;
        private CancellationTokenSource? _cancellationTokenSource;

        public SearchContainer TargetContainer { get; }
        public bool Searching { get; private set; }

        protected SearchTaskBase(IContainer searchBaseContainer)
        {
            TargetContainer = new SearchContainer(searchBaseContainer, this);
            _baseContainer = searchBaseContainer;
        }

        public void Start()
        {
            lock (_searchGuard)
            {
                if (Searching) return;
                Searching = true;
                try
                {
                    _cancellationTokenSource?.Cancel();
                }
                catch { }
            }

            new Thread(BootstrapSearch).Start();

            void BootstrapSearch()
            {
                try
                {
                    Task.Run(Search).Wait();
                }
                finally
                {
                    lock (_searchGuard)
                    {
                        Searching = false;
                    }
                }
            }
        }

        private async Task Search()
        {
            try
            {
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    await TargetContainer.RunWithLazyLoading(async (token) => await TraverseTree(_baseContainer, token), _cancellationTokenSource.Token);
                }
            }
            finally
            {
                _cancellationTokenSource = null;
            }
        }

        private async Task TraverseTree(IContainer container, CancellationToken token = default)
        {
            if (token.IsCancellationRequested) return;

            await container.RefreshAsync(token);
            if (await IsItemMatch(container))
            {
                await AddContainer(container);
            }
            var childElements = await container.GetElements(token);
            var childContainers = await container.GetContainers(token);

            if (childElements != null)
            {
                await foreach (var childElement in
                    childElements
                        .ToAsyncEnumerable()
                        .WhereAwait(async e => await IsItemMatch(e))
                )
                {
                    if (token.IsCancellationRequested) return;
                    await AddElement(childElement);
                }
            }

            if (childContainers != null)
            {
                foreach (var childContainer in childContainers)
                {
                    await TraverseTree(childContainer, token);
                }
            }
        }

        private async Task AddContainer(IContainer container)
        {
            await TargetContainer.AddContainer(container);
        }

        private async Task AddElement(IElement element)
        {
            await TargetContainer.AddElement(element);
        }

        protected abstract Task<bool> IsItemMatch(IItem item);

        public virtual List<ItemNamePart> GetDisplayName(IItem item)
        {
            return new List<ItemNamePart>()
            {
                new ItemNamePart(item.Name)
            };
        }

        public bool Cancel()
        {
            if (!Searching || _cancellationTokenSource == null) return false;
            _cancellationTokenSource?.Cancel();
            return true;
        }
    }
}