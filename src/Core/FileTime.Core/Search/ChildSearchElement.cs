using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.Search
{
    public class ChildSearchElement : AbstractElement<IContentProvider>
    {
        public IElement BaseElement { get; }

        public ChildSearchElement(SearchContainer searchContainer, IContentProvider provider, IContainer parent, IElement baseElement, string name, List<ItemNamePart> searchDisplayName) : base(provider, parent, name)
        {
            DisplayName = baseElement.DisplayName;
            NativePath = FullName;
            SearchContainer = searchContainer;
            SearchDisplayName = searchDisplayName;
            BaseElement = baseElement;
        }

        public SearchContainer SearchContainer { get; }
        public List<ItemNamePart> SearchDisplayName { get; }

        public override Task Delete(bool hardDelete = false) => throw new NotSupportedException();

        public override Task<string> GetContent(CancellationToken token = default) => throw new NotSupportedException();

        public override Task<IContentReader> GetContentReaderAsync() => throw new NotSupportedException();

        public override Task<IContentWriter> GetContentWriterAsync() => throw new NotSupportedException();

        public override async Task<long?> GetElementSize(CancellationToken token = default) => await BaseElement.GetElementSize(token);

        public override string GetPrimaryAttributeText() => BaseElement.GetPrimaryAttributeText();

        public override Task Rename(string newName) => throw new NotSupportedException();
    }
}