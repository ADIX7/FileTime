using System.Runtime.InteropServices;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Providers.Local
{
    public class LocalContentProvider : IContentProvider
    {
        public IReadOnlyList<IContainer> RootContainers { get; }

        public IReadOnlyList<IItem> Items => RootContainers;

        public IReadOnlyList<IContainer> Containers => RootContainers;

        public IReadOnlyList<IElement> Elements { get; } = new List<IElement>();

        public string Name { get; } = "local";

        public string? FullName { get; }
        public bool IsHidden => false;

        public IContentProvider Provider => this;

        public event EventHandler? Refreshed;

        public LocalContentProvider()
        {
            var rootDirectories = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new DirectoryInfo("/").GetDirectories()
                : Environment.GetLogicalDrives().Select(d => new DirectoryInfo(d));

            FullName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "" : null;

            RootContainers = rootDirectories.Select(d => new LocalFolder(d, this, this)).OrderBy(d => d.Name).ToList().AsReadOnly();
        }

        public IItem? GetByPath(string path)
        {
            var pathParts = path.TrimStart(Constants.SeparatorChar).Split(Constants.SeparatorChar);
            var rootContainer = RootContainers.FirstOrDefault(c => c.Name == pathParts[0]);

            if (rootContainer == null) return null;

            return rootContainer.GetByPath(string.Join(Constants.SeparatorChar, pathParts.Skip(1)));
        }

        public void Refresh()
        {
        }

        public IContainer? GetParent()
        {
            return null;
        }
        public IContainer CreateContainer(string name) => throw new NotSupportedException();
        public IElement CreateElement(string name) => throw new NotSupportedException();
        public bool IsExists(string name) => Items.Any(i => i.Name == name);

        public void Delete() => throw new NotSupportedException();
    }
}