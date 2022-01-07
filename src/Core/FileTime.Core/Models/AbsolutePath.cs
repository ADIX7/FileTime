using FileTime.Core.Providers;

namespace FileTime.Core.Models
{
    public class AbsolutePath : IAbsolutePath
    {
        public IContentProvider ContentProvider { get; }

        public string Path { get; }

        public AbsolutePath(IContentProvider contentProvider, string path)
        {
            ContentProvider = contentProvider;
            Path = path;
        }
    }
}