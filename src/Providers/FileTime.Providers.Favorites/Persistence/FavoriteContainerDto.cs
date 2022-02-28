namespace FileTime.Providers.Favorites.Persistence
{
    public class FavoriteContainerDto
    {
        public List<FavoriteContainerDto> Containers { get; set; } = new();
        public List<FavoriteElementDto> Elements { get; set; } = new();
        public string Name { get; set; } = null!;

        public async Task Init(FavoriteContainer favoriteContainer)
        {
            Name = favoriteContainer.Name;

            var newContainers = new List<FavoriteContainerDto>();
            var newElements = new List<FavoriteElementDto>();

            var containers = await favoriteContainer.GetContainers();
            var elements = await favoriteContainer.GetElements();

            if (containers != null)
            {
                foreach (var container in containers.Cast<FavoriteContainer>())
                {
                    var childFavorite = new FavoriteContainerDto();
                    await childFavorite.Init(container);
                    newContainers.Add(childFavorite);
                }
            }

            if (elements != null)
            {
                foreach (var element in elements.Cast<FavoriteElement>())
                {
                    var childFavorite = new FavoriteElementDto(element);
                    newElements.Add(childFavorite);
                }
            }

            Containers = newContainers;
            Elements = newElements;
        }
    }
}