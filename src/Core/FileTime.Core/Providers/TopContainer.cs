
using System;
using AsyncEvent;
using FileTime.Core.Models;

namespace FileTime.Core.Providers
{
    public class TopContainer : IContainer
    {
        private readonly List<IContentProvider> _contentProviders;
        private readonly IReadOnlyList<IContainer>? _containers;
        private readonly IReadOnlyList<IItem>? _items;
        private readonly IReadOnlyList<IElement>? _elements = new List<IElement>().AsReadOnly();

        public string Name => null;

        public string? FullName => null;

        public bool IsHidden => false;
        public bool IsLoaded => true;

        public IContentProvider Provider => null;
        public bool CanDelete => false;
        public bool CanRename => false;

        public AsyncEventHandler Refreshed { get; } = new();

        public TopContainer(IEnumerable<IContentProvider> contentProviders)
        {
            _contentProviders = new List<IContentProvider>(contentProviders);
            _containers = _contentProviders.AsReadOnly();
            _items = _containers.Cast<IItem>().ToList().AsReadOnly();

            foreach (var contentProvider in contentProviders)
            {
                contentProvider.SetParent(this);
            }
        }

        public Task<IContainer> CreateContainer(string name) => throw new NotImplementedException();

        public Task<IElement> CreateElement(string name) => throw new NotImplementedException();

        public Task Delete() => throw new NotImplementedException();

        public Task<IItem?> GetByPath(string path) => throw new NotImplementedException();

        public IContainer? GetParent() => null;

        public Task<bool> IsExists(string name) => throw new NotImplementedException();

        public async Task Refresh() => await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty);

        public Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default) => Task.FromResult(_items);
        public Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default) => Task.FromResult(_containers);
        public Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default) => Task.FromResult(_elements);

        public Task<IContainer> Clone() => Task.FromResult((IContainer)this);

        public Task Rename(string newName) => throw new NotSupportedException();
    }
}