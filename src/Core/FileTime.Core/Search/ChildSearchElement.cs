using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.Search
{
    public class ChildSearchElement : AbstractElement<IContentProvider>
    {
        public ChildSearchElement(SearchContainer searchContainer, IContentProvider provider, IContainer parent, string name, string displayName, List<ItemNamePart> searchDisplayName) : base(provider, parent, name)
        {
            DisplayName = displayName;
            NativePath = FullName;
            SearchContainer = searchContainer;
            SearchDisplayName = searchDisplayName;
        }

        public SearchContainer SearchContainer { get; }
        public List<ItemNamePart> SearchDisplayName { get; }

        public override Task Delete(bool hardDelete = false) => throw new NotSupportedException();

        public override Task<string> GetContent(CancellationToken token = default) => throw new NotSupportedException();

        public override Task<IContentReader> GetContentReaderAsync() => throw new NotSupportedException();

        public override Task<IContentWriter> GetContentWriterAsync() => throw new NotSupportedException();

        public override Task<long?> GetElementSize(CancellationToken token = default) => Task.FromResult((long?)null);

        public override string GetPrimaryAttributeText() => "";

        public override Task Rename(string newName) => throw new NotSupportedException();
    }
}