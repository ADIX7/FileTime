using System.Runtime.InteropServices;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Local
{
    public class LocalContentProvider : IContentProvider
    {
        private readonly ILogger<LocalContentProvider> _logger;

        public IReadOnlyList<IContainer> RootContainers { get; }

        public IReadOnlyList<IItem> Items => RootContainers;

        public IReadOnlyList<IContainer> Containers => RootContainers;

        public IReadOnlyList<IElement> Elements { get; } = new List<IElement>();

        public string Name { get; } = "local";

        public string? FullName { get; }
        public bool IsHidden => false;

        public IContentProvider Provider => this;

        public event EventHandler? Refreshed;

        public bool IsCaseInsensitive { get; }

        public LocalContentProvider(ILogger<LocalContentProvider> logger)
        {
            _logger = logger;

            IsCaseInsensitive = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var rootDirectories = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new DirectoryInfo("/").GetDirectories()
                : Environment.GetLogicalDrives().Select(d => new DirectoryInfo(d));

            FullName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "" : null;

            RootContainers = rootDirectories.Select(d => new LocalFolder(d, this, this)).OrderBy(d => d.Name).ToList().AsReadOnly();
        }

        public IItem? GetByPath(string path)
        {
            var pathParts = (IsCaseInsensitive ? path.ToLower() : path).TrimStart(Constants.SeparatorChar).Split(Constants.SeparatorChar);
            var rootContainer = RootContainers.FirstOrDefault(c => NormalizePath(c.Name) == NormalizePath(pathParts[0]));

            _logger.LogError("No root container found with name '{0}'", path[0]);
            if (rootContainer == null)
            {
                _logger.LogWarning("No root container found with name '{0}'", path[0]);
                return null;
            }

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

        internal string NormalizePath(string path) => IsCaseInsensitive ? path.ToLower() : path;
    }
}