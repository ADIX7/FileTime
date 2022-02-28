using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Providers.Favorites
{
    public class FavoriteElement : AbstractElement<FavoriteContentProvider>, ISymlinkElement, IFavoriteItem
    {
        public IItem BaseItem { get; }
        public bool IsPinned { get; set; }

        IItem ISymlinkElement.RealItem => BaseItem;

        public FavoriteElement(FavoriteContainerBase parent, string name, IItem baseItem, bool isPinned = false) : base(parent.Provider, parent, name)
        {
            BaseItem = baseItem;
            IsPinned = isPinned;
        }

        public override Task Delete(bool hardDelete = false) => throw new NotSupportedException();

        public override Task<string> GetContent(CancellationToken token = default) => throw new NotSupportedException();

        public override Task<IContentReader> GetContentReaderAsync() => throw new NotSupportedException();

        public override Task<IContentWriter> GetContentWriterAsync() => throw new NotSupportedException();

        public override Task<long?> GetElementSize(CancellationToken token = default) => Task.FromResult((long?)null);

        public override string? GetPrimaryAttributeText() => null;

        public override Task Rename(string newName) => throw new NotSupportedException();
    }
}