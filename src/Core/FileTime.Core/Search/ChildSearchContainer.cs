using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.Search
{
    public class ChildSearchContainer : AbstractContainer<IContentProvider>
    {
        public ChildSearchContainer(SearchContainer searchContainer, IContentProvider provider, IContainer baseContainer, string name, string displayName, List<ItemNamePart> searchDisplayName) : base(provider, baseContainer.GetParent()!, name)
        {
            DisplayName = displayName;
            SearchContainer = searchContainer;
            SearchDisplayName = searchDisplayName;
            BaseContainer = baseContainer;
        }

        public override bool IsExists => true;

        public SearchContainer SearchContainer { get; }
        public List<ItemNamePart> SearchDisplayName { get; }
        public IContainer BaseContainer { get; }

        public override async Task RefreshAsync(CancellationToken token = default)
        {
            if (Refreshed != null) await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        }

        public override Task<IContainer> CloneAsync() => throw new NotImplementedException();

        public override Task<IContainer> CreateContainerAsync(string name) => throw new NotSupportedException();

        public override Task<IElement> CreateElementAsync(string name) => throw new NotSupportedException();

        public override Task Delete(bool hardDelete = false) => throw new NotSupportedException();

        public override Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default) => throw new NotImplementedException();

        public override Task Rename(string newName) => throw new NotSupportedException();
    }
}