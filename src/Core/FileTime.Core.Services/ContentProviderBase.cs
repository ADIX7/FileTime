using FileTime.Core.Enums;
using FileTime.Core.Models;

namespace FileTime.Core.Services
{
    public abstract class ContentProviderBase : IContentProvider
    {
        protected List<IAbsolutePath> Items { get; set; } = new List<IAbsolutePath>();

        IReadOnlyList<IAbsolutePath> IContainer.Items => Items;

        public string Name { get; }

        public string DisplayName { get; }

        public FullName? FullName => null;

        public NativePath? NativePath => null;

        public bool IsHidden => false;

        public bool IsExists => true;

        public SupportsDelete CanDelete => SupportsDelete.False;

        public bool CanRename => false;

        public IContentProvider Provider => this;

        public FullName? Parent => null;

        protected ContentProviderBase(string name)
        {
            DisplayName = Name = name;
        }

        public virtual Task OnEnter() => Task.CompletedTask;
        public virtual async Task<IItem> GetItemByFullNameAsync(FullName fullName) => await GetItemByNativePathAsync(GetNativePath(fullName));
        public abstract Task<IItem> GetItemByNativePathAsync(NativePath nativePath);
        public abstract Task<List<IAbsolutePath>> GetItemsByContainerAsync(FullName fullName);
        public abstract NativePath GetNativePath(FullName fullName);
    }
}