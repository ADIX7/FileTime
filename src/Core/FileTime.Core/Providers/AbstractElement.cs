using FileTime.Core.Models;

namespace FileTime.Core.Providers
{
    public abstract class AbstractElement<TProvider> : IElement where TProvider : class, IContentProvider
    {
        private readonly IContainer _parent;
        public bool IsSpecial { get; protected set; }

        public string Name { get; protected set; }

        public string DisplayName { get; protected set; }

        public string? FullName { get; protected set; }

        public string? NativePath { get; protected set; }

        public bool IsHidden { get; protected set; }

        public bool IsDestroyed { get; protected set; }

        public virtual bool IsExists { get; protected set; }

        public virtual SupportsDelete CanDelete { get; protected set; }

        public virtual bool CanRename { get; protected set; }

        public TProvider Provider { get; }

        IContentProvider IItem.Provider => Provider;

        protected AbstractElement(TProvider provider, IContainer parent, string name)
        {
            _parent = parent;
            Provider = provider;
            DisplayName = Name = name;
            FullName = parent.FullName + Constants.SeparatorChar + name;
            IsExists = true;
        }

        public abstract Task Delete(bool hardDelete = false);

        public virtual void Destroy() { }

        public abstract Task<string> GetContent(CancellationToken token = default);

        public abstract Task<IContentReader> GetContentReaderAsync();

        public abstract Task<IContentWriter> GetContentWriterAsync();

        public abstract Task<long?> GetElementSize(CancellationToken token = default);

        public IContainer? GetParent() => _parent;
        public abstract string GetPrimaryAttributeText();

        public abstract Task Rename(string newName);
    }
}