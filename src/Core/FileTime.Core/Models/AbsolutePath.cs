using FileTime.Core.Providers;
using FileTime.Core.Timeline;

namespace FileTime.Core.Models
{
    public sealed class AbsolutePath : IEquatable<AbsolutePath>
    {
        public IContentProvider ContentProvider { get; }
        public IContentProvider? VirtualContentProvider { get; }

        public string Path { get; }
        public AbsolutePathType Type { get; }

        public AbsolutePath(AbsolutePath from)
        {
            ContentProvider = from.ContentProvider;
            Path = from.Path;
            VirtualContentProvider = from.VirtualContentProvider;
            Type = from.Type;
        }

        public AbsolutePath(IContentProvider contentProvider, string path, AbsolutePathType type, IContentProvider? virtualContentProvider)
        {
            ContentProvider = contentProvider;
            Path = path;
            VirtualContentProvider = virtualContentProvider;
            Type = type;
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

            Type = item switch
            {
                IContainer => AbsolutePathType.Container,
                IElement => AbsolutePathType.Element,
                _ => AbsolutePathType.Unknown
            };
        }

        public static AbsolutePath FromParentAndChildName(IContainer parent, string childName, AbsolutePathType childType)
        {
            var contentProvider = parent.Provider;
            var path = parent.FullName! + Constants.SeparatorChar + childName;

            var virtualContentProvider = parent switch
            {
                TimeContainer timeContainer => timeContainer.VirtualProvider,
                _ => null
            };

            return new AbsolutePath(contentProvider, path, childType, virtualContentProvider);
        }

        public AbsolutePath GetChild(string childName, AbsolutePathType childType)
        {
            var path = Path + Constants.SeparatorChar + childName;

            return new AbsolutePath(ContentProvider, path, childType, VirtualContentProvider);
        }

        public async Task<IItem?> ResolveAsync()
        {
            var result = VirtualContentProvider != null && (await VirtualContentProvider.IsExistsAsync(Path))
                ? await VirtualContentProvider.GetByPath(Path)
                : null;

            result ??= await ContentProvider.GetByPath(Path);

            return result;
        }

        public string GetParentPath()
        {
            var pathParts = Path.Split(Constants.SeparatorChar);
            return string.Join(Constants.SeparatorChar, pathParts[..^1]);
        }

        public AbsolutePath GetParent() => new(ContentProvider, GetParentPath(), AbsolutePathType.Container, VirtualContentProvider);

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