using System.Runtime.InteropServices;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Local
{
    public class LocalContentProvider : ContentProviderBase<LocalContentProvider>
    {
        private readonly ILogger<LocalContentProvider> _logger;
        public bool IsCaseInsensitive { get; }

        public LocalContentProvider(ILogger<LocalContentProvider> logger)
        : base("local", "local://", true)
        {
            _logger = logger;

            IsCaseInsensitive = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var rootDirectories = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new DirectoryInfo("/").GetDirectories()
                : Environment.GetLogicalDrives().Select(d => new DirectoryInfo(d));

            SetRootContainers(rootDirectories.Select(d => new LocalFolder(d, this, this)).OrderBy(d => d.Name)).Wait();
        }

        public async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            path = path.Replace(Path.DirectorySeparatorChar, Constants.SeparatorChar).TrimEnd(Constants.SeparatorChar);
            var pathParts = (IsCaseInsensitive ? path.ToLower() : path).TrimStart(Constants.SeparatorChar).Split(Constants.SeparatorChar);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && pathParts.Length == 1 && pathParts[0]?.Length == 0) return this;

            var normalizedRootContainerName = NormalizePath(pathParts[0]);
            var rootContainer = RootContainers?.FirstOrDefault(c => NormalizePath(c.Name) == normalizedRootContainerName);

            if (rootContainer == null)
            {
                _logger.LogWarning("No root container found with name '{RootContainerName}'.", path[0]);
                return null;
            }

            var remainingPath = string.Join(Constants.SeparatorChar, pathParts.Skip(1));
            return remainingPath.Length == 0 ? rootContainer : await rootContainer.GetByPath(remainingPath, acceptDeepestMatch);
        }

        public override Task<IContainer> CreateContainerAsync(string name) => throw new NotSupportedException();
        public override Task<IElement> CreateElementAsync(string name) => throw new NotSupportedException();

        internal string NormalizePath(string path) => IsCaseInsensitive ? path.ToLower() : path;

        public override async Task<bool> CanHandlePath(string path)
        {
            var normalizedPath = NormalizePath(path);
            Func<IContainer, bool> match = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? c => normalizedPath.StartsWith(NormalizePath(c.Name))
                : c => normalizedPath.StartsWith(NormalizePath("/" + c.Name));
            return (await GetContainers())?.Any(match) ?? false;
        }
    }
}