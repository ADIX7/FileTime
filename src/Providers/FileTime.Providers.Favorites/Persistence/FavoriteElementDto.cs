using FileTime.Core.Models;

namespace FileTime.Providers.Favorites.Persistence
{
    public class FavoriteElementDto
    {
        public string Name { get; set; } = null!;
        public AbsolutePathDto RealPath { get; set; } = null!;
        public bool IsPinned { get; set; }

        public FavoriteElementDto() { }

        public FavoriteElementDto(FavoriteElement element)
        {
            Name = element.Name;
            RealPath = new AbsolutePathDto(new AbsolutePath(element.BaseItem));
            IsPinned = element.IsPinned;
        }
    }
}