using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Providers.Smb
{
    public class SmbContentProvider : IContentProvider
    {
        private IContainer _parent;
        private readonly List<IContainer> _rootContainers;
        private readonly IInputInterface _inputInterface;

        public IReadOnlyList<IContainer> RootContainers { get; }

        public IReadOnlyList<IItem> Items => RootContainers;

        public IReadOnlyList<IContainer> Containers => RootContainers;

        public IReadOnlyList<IElement> Elements { get; } = new List<IElement>();

        public string Name { get; } = "smb";

        public string? FullName { get; }

        public bool IsHidden => false;

        public IContentProvider Provider => this;

        public event EventHandler? Refreshed;

        public SmbContentProvider(IInputInterface inputInterface)
        {
            _rootContainers = new List<IContainer>();
            RootContainers = _rootContainers.AsReadOnly();
            _inputInterface = inputInterface;
        }

        public IContainer CreateContainer(string name)
        {
            var fullName = "\\\\" + name;
            var container = _rootContainers.Find(c => c.Name == name);

            if (container == null)
            {
                container = new SmbServer(fullName, this, _inputInterface);
                _rootContainers.Add(container);
            }

            Refresh();

            return container;
        }

        public IElement CreateElement(string name)
        {
            throw new NotSupportedException();
        }

        public void Delete()
        {
            throw new NotSupportedException();
        }

        public IItem? GetByPath(string path)
        {
            throw new NotImplementedException();
        }

        public IContainer? GetParent() => _parent;

        public bool IsExists(string name) => Items.Any(i => i.Name == name);

        public void Refresh()
        {
            Refreshed?.Invoke(this, EventArgs.Empty);
        }

        public bool CanHandlePath(string path) => path.StartsWith("smb://") || path.StartsWith(@"\\");

        public void SetParent(IContainer container) => _parent = container;
    }
}