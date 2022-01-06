using FileTime.Core.Providers;

namespace FileTime.Core.Models
{
    public class VirtualContainer : IContainer
    {
        private readonly List<Func<IEnumerable<IContainer>, IEnumerable<IContainer>>> _containerTransformators;
        private readonly List<Func<IEnumerable<IElement>, IEnumerable<IElement>>> _elementTransformators;

        public IContainer BaseContainer { get; }

        public bool IsPermanent { get; }
        public bool IsTransitive { get; }
        public string? VirtualContainerName { get; }
        public IReadOnlyList<IItem> Items { get; private set; }

        public IReadOnlyList<IContainer> Containers { get; private set; }

        public IReadOnlyList<IElement> Elements { get; private set; }

        public string Name => BaseContainer.Name;

        public string? FullName => BaseContainer.FullName;

        public bool IsHidden => BaseContainer.IsHidden;

        public IContentProvider Provider => BaseContainer.Provider;

        public event EventHandler? Refreshed
        {
            add => BaseContainer.Refreshed += value;
            remove => BaseContainer.Refreshed -= value;
        }

        public VirtualContainer(
            IContainer baseContainer,
            List<Func<IEnumerable<IContainer>, IEnumerable<IContainer>>> containerTransformators,
            List<Func<IEnumerable<IElement>, IEnumerable<IElement>>> elementTransformators,
            bool isPermanent = false,
            bool isTransitive = false,
            string? virtualContainerName = null)
        {
            BaseContainer = baseContainer;
            _containerTransformators = containerTransformators;
            _elementTransformators = elementTransformators;

            InitItems();
            IsPermanent = isPermanent;
            IsTransitive = isTransitive;
            VirtualContainerName = virtualContainerName;
        }

        private void InitItems()
        {
            Containers = _containerTransformators.Aggregate(BaseContainer.Containers.AsEnumerable(), (a, t) => t(a)).ToList().AsReadOnly();
            Elements = _elementTransformators.Aggregate(BaseContainer.Elements.AsEnumerable(), (a, t) => t(a)).ToList().AsReadOnly();

            Items = Containers.Cast<IItem>().Concat(Elements).ToList().AsReadOnly();
        }

        public IItem? GetByPath(string path) => BaseContainer.GetByPath(path);

        public IContainer? GetParent() => BaseContainer.GetParent();

        public void Refresh()
        {
            BaseContainer.Refresh();
            InitItems();
        }

        public IContainer GetRealContainer() =>
            BaseContainer is VirtualContainer virtualContainer ? virtualContainer.GetRealContainer() : BaseContainer;

        public bool HasWithName(string name) =>
            VirtualContainerName == name
            || (BaseContainer is VirtualContainer virtualContainer
                && virtualContainer.HasWithName(name));

        public IContainer ExceptWithName(string name)
        {
            if (BaseContainer is VirtualContainer virtualBaseContainer && virtualBaseContainer.VirtualContainerName == name)
            {
                return new VirtualContainer(
                    virtualBaseContainer.ExceptWithName(name),
                    _containerTransformators,
                    _elementTransformators,
                    IsPermanent,
                    IsTransitive,
                    VirtualContainerName);
            }
            else if (VirtualContainerName == name)
            {
                return BaseContainer;
            }

            return this;
        }

        public IContainer CloneVirtualChainFor(IContainer container, Func<VirtualContainer, bool> predicate)
        {
            var baseContainer = BaseContainer is VirtualContainer baseVirtualContainer
                ? baseVirtualContainer.CloneVirtualChainFor(container, predicate)
                : container;

            return predicate(this)
                ? new VirtualContainer(
                        baseContainer,
                        _containerTransformators,
                        _elementTransformators,
                        IsPermanent,
                        IsTransitive,
                        VirtualContainerName)
                : baseContainer;
        }

        public IContainer CreateContainer(string name) => BaseContainer.CreateContainer(name);
        public IElement CreateElement(string name) => BaseContainer.CreateElement(name);
        public bool IsExists(string name) => BaseContainer.IsExists(name);

        public void Delete() => BaseContainer.Delete();
    }
}