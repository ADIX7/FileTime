using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.App.Core.Tab
{
    public class TabItem : IAbsolutePath
    {
        public IContentProvider ContentProvider { get; }
        public string Path { get; }

        public TabItem(IContentProvider contentProvider, string path)
        {
            ContentProvider = contentProvider;
            Path = path;
        }
    }
}