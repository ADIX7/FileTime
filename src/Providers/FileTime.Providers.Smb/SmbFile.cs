using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Providers.Smb
{
    public class SmbFile : IElement
    {
        public bool IsSpecial => false;

        public string Name { get; }

        public string? FullName { get; }
        public string? NativePath { get; }

        public bool IsHidden => false;
        public SupportsDelete CanDelete => SupportsDelete.True;
        public bool CanRename => true;

        public IContentProvider Provider { get; }
        private IContainer _parent;

        public bool IsDestroyed { get; private set; }

        public SmbFile(string name, SmbContentProvider provider, IContainer parent)
        {
            Name = name;
            FullName = parent.FullName + Constants.SeparatorChar + Name;
            NativePath = SmbContentProvider.GetNativePath(FullName);

            Provider = provider;
            _parent = parent;
        }

        public Task Delete(bool hardDelete = false)
        {
            throw new NotImplementedException();
        }
        public Task Rename(string newName)
        {
            throw new NotImplementedException();
        }

        public string GetPrimaryAttributeText()
        {
            return "";
        }

        public IContainer? GetParent() => _parent;
        public Task<string> GetContent(CancellationToken token = default) => Task.FromResult("NotImplemented");
        public Task<long> GetElementSize(CancellationToken token = default) => Task.FromResult(-1L);

        public void Destroy() => IsDestroyed = true;
    }
}