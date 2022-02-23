using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.ContainerSizeScanner
{
    public class ContainerSizeElement : AbstractElement<ContainerScanSnapshotProvider>, IItemWithSize, IFile
    {
        private readonly IElement _element;

        public long Size { get; }

        long? IItemWithSize.Size => Size;

        public string Attributes => _element is IFile file ? file.Attributes : _element.GetType().Name.Split('.').Last();

        public DateTime CreatedAt => _element is IFile file ? file.CreatedAt : DateTime.MinValue;

        public ContainerSizeElement(ContainerScanSnapshotProvider provider, IContainer parent, IElement element, long size) : base(provider, parent, element.Name)
        {
            Size = size;
            CanDelete = SupportsDelete.False;
            _element = element;
        }

        public override Task Delete(bool hardDelete = false) => throw new NotSupportedException();

        public override Task<string> GetContent(CancellationToken token = default) => Task.FromResult("NotImplementedException");

        public override Task<IContentReader> GetContentReaderAsync() => throw new NotSupportedException();

        public override Task<IContentWriter> GetContentWriterAsync() => throw new NotSupportedException();

        public override Task<long?> GetElementSize(CancellationToken token = default) => Task.FromResult((long?)Size);

        public override string GetPrimaryAttributeText() => "";

        public override Task Rename(string newName) => throw new NotSupportedException();
    }
}