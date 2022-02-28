namespace FileTime.Providers.Favorites.Persistence
{
    public class FavoritePersistenceRoot
    {
        public List<FavoriteContainerDto> Containers { get; set; } = new();
        public List<FavoriteElementDto> Elements { get; set; } = new();
    }
}