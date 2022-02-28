using FileTime.Core.Providers;

namespace FileTime.Core.Models
{
    public class AbsolutePathDto
    {
        public string? Path { get; set; }
        public AbsolutePathType? Type { get; set; }
        public string? ContentProviderName { get; set; }
        public string? VirtualContentProviderName { get; set; }

        public AbsolutePathDto() { }
        public AbsolutePathDto(AbsolutePath path)
        {
            Path = path.Path;
            Type = path.Type;
            ContentProviderName = path.ContentProvider.Name;
            VirtualContentProviderName = path.VirtualContentProvider?.Name;
        }

        public AbsolutePath Resolve(IEnumerable<IContentProvider> providers)
        {
            var contentProvider = providers.FirstOrDefault(p => p.Name == ContentProviderName) ?? throw new Exception($"Could not found content provider with name {ContentProviderName}");
            var virtualContentProvider = VirtualContentProviderName != null ? providers.FirstOrDefault(p => p.Name == VirtualContentProviderName) : null;

            if (Path is null) throw new Exception(nameof(Path) + " can not be null.");
            if (Type is not AbsolutePathType type) throw new Exception(nameof(Type) + " can not be null.");
            return new AbsolutePath(contentProvider, Path, type, virtualContentProvider);
        }
    }
}