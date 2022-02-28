using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Providers.Favorites.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Providers.Favorites
{
    public class FavoriteContentProvider : FavoriteContainerBase, IContentProvider
    {
        private bool _initialized;
        private readonly PersistenceService _persistenceService;
        private readonly Lazy<IEnumerable<IContentProvider>> _contentProvidersLazy;

        public FavoriteContentProvider(PersistenceService persistenceService, IServiceProvider serviceProvider) : base(persistenceService, "favorite")
        {
            Protocol = "favorite://";
            _persistenceService = persistenceService;
            _contentProvidersLazy = new Lazy<IEnumerable<IContentProvider>>(() => serviceProvider.GetRequiredService<IEnumerable<IContentProvider>>());
        }

        public bool SupportsContentStreams => false;

        public string Protocol { get; }

        public Task<bool> CanHandlePath(string path) => Task.FromResult(path.StartsWith(Protocol));

        public void SetParent(IContainer container)
        {
            Parent = container;
        }

        public async Task SaveAsync()
        {
            await SaveFavoritesAsync();
        }

        public override async Task InitIfNeeded()
        {
            if (!_initialized)
            {
                _initialized = true;
                var (containerDtos, elementDtos) = await _persistenceService.LoadFavorites();

                await AddItems(this, containerDtos, elementDtos);
            }
        }

        private async Task AddItems(
            FavoriteContainerBase container,
            IEnumerable<FavoriteContainerDto> containerDtos,
            IEnumerable<FavoriteElementDto> elementDtos)
        {
            var newContainers = new List<FavoriteContainer>();
            var newElements = new List<FavoriteElement>();

            foreach (var containerDto in containerDtos)
            {
                var newContainer = new FavoriteContainer(_persistenceService, this, container, containerDto.Name);
                newContainers.Add(newContainer);

                await AddItems(newContainer, containerDto.Containers, containerDto.Elements);
            }

            foreach (var elementDto in elementDtos)
            {
                var item = await elementDto.RealPath.Resolve(_contentProvidersLazy.Value).ResolveAsync();
                if (item is not null)
                {
                    var newElement = new FavoriteElement(container, elementDto.Name, item, elementDto.IsPinned);
                    newElements.Add(newElement);
                }
            }

            await container.AddContainersAsync(newContainers);
            await container.AddElementsAsync(newElements);
        }
    }
}