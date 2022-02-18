using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.Timeline
{
    public class TimeElement : IElement
    {
        private readonly IContainer _parent;
        public TimeElement(string name, TimeContainer parent, IContentProvider contentProvider, IContentProvider virtualContentProvider)
        {
            _parent = parent;

            Name = name;
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
            Provider = contentProvider;
            VirtualProvider = virtualContentProvider;
        }

        public bool IsSpecial => false;

        public string Name { get; }

        public string? FullName { get; }

        //FIXME: currently this can be different of the real items NativePath, should be fixed
        public string? NativePath => FullName;

        public bool IsHidden => false;

        public SupportsDelete CanDelete => SupportsDelete.True;

        public bool CanRename => true;

        public IContentProvider Provider { get; }
        public IContentProvider VirtualProvider { get; }

        public bool IsDestroyed { get; private set; }
        public bool IsExists => true;

        public Task Delete(bool hardDelete = false) => Task.CompletedTask;

        public IContainer? GetParent() => _parent;

        public string GetPrimaryAttributeText() => "";

        public Task Rename(string newName) => Task.CompletedTask;

        public Task<string> GetContent(CancellationToken token = default) => Task.FromResult("");
        public Task<long> GetElementSize(CancellationToken token = default) => Task.FromResult(-1L);

        public void Destroy() => IsDestroyed = true;

        public Task<IContentReader> GetContentReaderAsync() => throw new NotSupportedException();

        public Task<IContentWriter> GetContentWriterAsync() => throw new NotSupportedException();
    }
}