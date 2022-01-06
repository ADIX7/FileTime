using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.App.Core.Clipboard
{
    public class ClipboardItem : IAbsolutePath
    {
        public IContentProvider ContentProvider { get; }
        public string Path { get; }

        public ClipboardItem(IContentProvider contentProvider, string path)
        {
            ContentProvider = contentProvider;
            Path = path;
        }
    }
}