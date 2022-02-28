using FileTime.Core.Models;
using FileTime.Providers.Favorites.Persistence;

namespace FileTime.Providers.Favorites
{
    public class FavoriteContainer : FavoriteContainerBase, IFavoriteItem
    {
        public FavoriteContainer(PersistenceService persistenceService, FavoriteContentProvider provider, IContainer parent, string name) : base(persistenceService, provider, parent, name)
        {
        }
    }
}