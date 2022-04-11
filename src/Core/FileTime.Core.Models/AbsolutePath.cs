using FileTime.Core.Enums;
using FileTime.Core.Services;

namespace FileTime.Core.Models
{
    public class AbsolutePath : IAbsolutePath
    {
        public IContentProvider ContentProvider { get; }
        public IContentProvider? VirtualContentProvider { get; }

        public FullName Path { get; }
        public AbsolutePathType Type { get; }

        public AbsolutePath(IContentProvider contentProvider, FullName path, AbsolutePathType type, IContentProvider? virtualContentProvider = null)
        {
            ContentProvider = contentProvider;
            Path = path;
            VirtualContentProvider = virtualContentProvider;
            Type = type;
        }

        public async Task<IItem> ResolveAsync()
        {
            var provider = VirtualContentProvider ?? ContentProvider;
            return await provider.GetItemByFullNameAsync(Path);
        }
    }
}