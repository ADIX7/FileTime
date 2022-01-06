using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.App.Core.Pane
{
    public class PaneItem : IAbsolutePath
    {
        public IContentProvider ContentProvider { get; }
        public string Path { get; }

        public PaneItem(IContentProvider contentProvider, string path)
        {
            ContentProvider = contentProvider;
            Path = path;
        }
    }
}