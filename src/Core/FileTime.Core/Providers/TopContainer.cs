
using FileTime.Core.Models;

namespace FileTime.Core.Providers
{
    public class TopContainer : IContainer
    {
        private readonly List<IContentProvider> _contentProviders;

        public IReadOnlyList<IItem> Items => Containers;

        public IReadOnlyList<IContainer> Containers { get; }

        public IReadOnlyList<IElement> Elements { get; } = new List<IElement>().AsReadOnly();

        public string Name => null;

        public string? FullName => null;

        public bool IsHidden => false;

        public IContentProvider Provider => null;

        public event EventHandler? Refreshed;

        public TopContainer(IEnumerable<IContentProvider> contentProviders)
        {
            _contentProviders = new List<IContentProvider>(contentProviders);
            Containers = _contentProviders.AsReadOnly();

            foreach (var contentProvider in contentProviders)
            {
                contentProvider.SetParent(this);
            }
        }

        public IContainer CreateContainer(string name) => throw new NotImplementedException();

        public IElement CreateElement(string name) => throw new NotImplementedException();

        public void Delete() => throw new NotImplementedException();

        public IItem? GetByPath(string path) => throw new NotImplementedException();

        public IContainer? GetParent() => null;

        public bool IsExists(string name) => throw new NotImplementedException();

        public void Refresh()
        {

        }
    }
}