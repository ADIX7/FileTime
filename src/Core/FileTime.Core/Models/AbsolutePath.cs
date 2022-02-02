using FileTime.Core.Providers;
using FileTime.Core.Timeline;

namespace FileTime.Core.Models
{
    public sealed class AbsolutePath : IEquatable<AbsolutePath>
    {
        public IContentProvider ContentProvider { get; }
        public IContentProvider? VirtualContentProvider { get; }

        public string Path { get; }

        public AbsolutePath(AbsolutePath from)
        {
            ContentProvider = from.ContentProvider;
            Path = from.Path;
            VirtualContentProvider = from.VirtualContentProvider;
        }

        public AbsolutePath(IContentProvider contentProvider, string path, IContentProvider? virtualContentProvider)
        {
            ContentProvider = contentProvider;
            Path = path;
            VirtualContentProvider = virtualContentProvider;
        }

        public AbsolutePath(IItem item)
        {
            if (item is TimeContainer timeContainer)
            {
                ContentProvider = timeContainer.Provider;
                VirtualContentProvider = timeContainer.VirtualProvider;
                Path = timeContainer.FullName!;
            }
            else if (item is TimeElement timeElement)
            {
                ContentProvider = timeElement.Provider;
                VirtualContentProvider = timeElement.VirtualProvider;
                Path = timeElement.FullName!;
            }
            else
            {
                ContentProvider = item.Provider;
                Path = item.FullName!;
            }
        }

        public static AbsolutePath FromParentAndChildName(IContainer parent, string childName)
        {
            IContentProvider? contentProvider;
            IContentProvider? virtualContentProvider;
            string? path;

            if (parent is TimeContainer timeContainer)
            {
                contentProvider = timeContainer.Provider;
                virtualContentProvider = timeContainer.VirtualProvider;
                path = timeContainer.FullName! + Constants.SeparatorChar + childName;
            }
            else
            {
                contentProvider = parent.Provider;
                path = parent.FullName! + Constants.SeparatorChar + childName;
                virtualContentProvider = null;
            }

            return new AbsolutePath(contentProvider, path, virtualContentProvider);
        }

        public async Task<IItem?> Resolve()
        {
            var result = VirtualContentProvider != null && (await VirtualContentProvider.IsExists(Path))
                ? await VirtualContentProvider.GetByPath(Path)
                : null;

            result ??= await ContentProvider.GetByPath(Path);

            return result;
        }

        public string GetParent()
        {
            var pathParts = Path.Split(Constants.SeparatorChar);
            return string.Join(Constants.SeparatorChar, pathParts[..^1]);
        }

        public AbsolutePath GetParentAsAbsolutePath() => new(ContentProvider, GetParent(), VirtualContentProvider);

        public string GetName() => Path.Split(Constants.SeparatorChar).Last();

        public override bool Equals(object? obj) => this.Equals(obj as AbsolutePath);

        public bool Equals(AbsolutePath? other) =>
            other is not null && other.ContentProvider == ContentProvider && other.Path == Path;

        public override int GetHashCode() => (ContentProvider.Name, Path).GetHashCode();

        public static bool operator ==(AbsolutePath? lhs, AbsolutePath? rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(AbsolutePath? lhs, AbsolutePath? rhs) => !(lhs == rhs);
    }
}