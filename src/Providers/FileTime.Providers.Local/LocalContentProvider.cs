using System;
using System.Runtime.InteropServices;
using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Local
{
    public class LocalContentProvider : IContentProvider
    {
        private readonly ILogger<LocalContentProvider> _logger;
        private IContainer? _parent;

        private readonly IReadOnlyList<IContainer> _rootContainers;
        private readonly IReadOnlyList<IItem>? _items;
        private readonly IReadOnlyList<IElement>? _elements = new List<IElement>().AsReadOnly();

        public string Name { get; } = "local";

        public string? FullName { get; }
        public bool IsHidden => false;
        public bool IsLoaded => true;

        public IContentProvider Provider => this;

        public AsyncEventHandler Refreshed { get; } = new();

        public bool IsCaseInsensitive { get; }
        public bool CanDelete => false;
        public bool CanRename => false;
        public IReadOnlyList<Exception> Exceptions { get; } = new List<Exception>().AsReadOnly();

        public LocalContentProvider(ILogger<LocalContentProvider> logger)
        {
            _logger = logger;

            IsCaseInsensitive = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var rootDirectories = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new DirectoryInfo("/").GetDirectories()
                : Environment.GetLogicalDrives().Select(d => new DirectoryInfo(d));

            FullName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "" : null;

            _rootContainers = rootDirectories.Select(d => new LocalFolder(d, this, this)).OrderBy(d => d.Name).ToList().AsReadOnly();
            _items = _rootContainers.Cast<IItem>().ToList().AsReadOnly();
        }

        public async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            path = path.Replace(Path.DirectorySeparatorChar, Constants.SeparatorChar).TrimEnd(Constants.SeparatorChar);
            var pathParts = (IsCaseInsensitive ? path.ToLower() : path).TrimStart(Constants.SeparatorChar).Split(Constants.SeparatorChar);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && pathParts.Length == 1 && pathParts[0] == "") return this;

            var normalizedRootContainerName = NormalizePath(pathParts[0]);
            var rootContainer = _rootContainers.FirstOrDefault(c => NormalizePath(c.Name) == normalizedRootContainerName);

            if (rootContainer == null)
            {
                _logger.LogWarning("No root container found with name '{0}'", path[0]);
                return null;
            }

            var remainingPath = string.Join(Constants.SeparatorChar, pathParts.Skip(1));
            return remainingPath.Length == 0 ? rootContainer : await rootContainer.GetByPath(remainingPath, acceptDeepestMatch);
        }

        public async Task Refresh() => await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty);

        public Task<IContainer> Clone() => Task.FromResult((IContainer)this);

        public IContainer? GetParent() => _parent;
        public Task<IContainer> CreateContainer(string name) => throw new NotSupportedException();
        public Task<IElement> CreateElement(string name) => throw new NotSupportedException();
        public Task<bool> IsExists(string name) => Task.FromResult(_rootContainers.Any(i => i.Name == name));

        public Task Delete() => throw new NotSupportedException();

        internal string NormalizePath(string path) => IsCaseInsensitive ? path.ToLower() : path;

        public bool CanHandlePath(string path)
        {
            var normalizedPath = NormalizePath(path);
            return _rootContainers.Any(r => normalizedPath.StartsWith(NormalizePath(r.Name)));
        }

        public void SetParent(IContainer container) => _parent = container;
        public Task<IReadOnlyList<IContainer>> GetRootContainers(CancellationToken token = default) => Task.FromResult(_rootContainers);

        public Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default) => Task.FromResult(_items);
        public Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default) => Task.FromResult((IReadOnlyList<IContainer>?)_rootContainers);
        public Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default) => Task.FromResult(_elements);

        public Task Rename(string newName) => throw new NotSupportedException();
        public Task<bool> CanOpen() => Task.FromResult(true);
    }
}