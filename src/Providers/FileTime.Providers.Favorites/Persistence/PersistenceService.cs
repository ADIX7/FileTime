using System.Text.Json;
using FileTime.Core.Persistence;

namespace FileTime.Providers.Favorites.Persistence
{
    public class PersistenceService
    {
        private const string favoriteFolderName = "favorites";
        private const string favoriteFileName = "favorites.json";
        private readonly PersistenceSettings _persistenceSettings;
        private readonly JsonSerializerOptions _jsonOptions;

        public PersistenceService(PersistenceSettings persistenceSettings)
        {
            _persistenceSettings = persistenceSettings;

            _jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task SaveFavorites(IEnumerable<IFavoriteItem> items)
        {
            var containers = new List<FavoriteContainerDto>();
            var elements = new List<FavoriteElementDto>();

            foreach (var favoriteItem in items)
            {
                if (favoriteItem is FavoriteContainer favoriteContainer)
                {
                    var childFavorite = new FavoriteContainerDto();
                    await childFavorite.Init(favoriteContainer);
                    containers.Add(childFavorite);
                }
                else if (favoriteItem is FavoriteElement favoriteElement)
                {
                    var childFavorite = new FavoriteElementDto(favoriteElement);
                    elements.Add(childFavorite);
                }
            }

            var root = new FavoritePersistenceRoot()
            {
                Containers = containers,
                Elements = elements
            };

            var favoriteDirectory = new DirectoryInfo(Path.Combine(_persistenceSettings.RootAppDataPath, favoriteFolderName));
            if (!favoriteDirectory.Exists) favoriteDirectory.Create();

            var persistencePath = Path.Combine(_persistenceSettings.RootAppDataPath, favoriteFolderName, favoriteFileName);

            using var stream = File.Create(persistencePath);
            await JsonSerializer.SerializeAsync(stream, root, _jsonOptions);
        }

        public async Task<(IEnumerable<FavoriteContainerDto>, IEnumerable<FavoriteElementDto>)> LoadFavorites()
        {
            var persistencePath = Path.Combine(_persistenceSettings.RootAppDataPath, favoriteFolderName, favoriteFileName);

            if (!new FileInfo(persistencePath).Exists) return (Enumerable.Empty<FavoriteContainerDto>(), Enumerable.Empty<FavoriteElementDto>());

            using var stream = File.OpenRead(persistencePath);
            var serversRoot = (await JsonSerializer.DeserializeAsync<FavoritePersistenceRoot>(stream))!;

            return (serversRoot.Containers, serversRoot.Elements);
        }
    }
}