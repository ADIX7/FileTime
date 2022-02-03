using AsyncEvent;
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
        public IReadOnlyList<IItem>? Items { get; private set; }

        public IReadOnlyList<IContainer>? Containers { get; private set; }

        public IReadOnlyList<IElement>? Elements { get; private set; }

        public string Name => BaseContainer.Name;

        public string? FullName => BaseContainer.FullName;

        public bool IsHidden => BaseContainer.IsHidden;
        public bool IsLoaded => BaseContainer.IsLoaded;
        public bool CanDelete => BaseContainer.CanDelete;
        public bool CanRename => BaseContainer.CanRename;

        public IContentProvider Provider => BaseContainer.Provider;
        public IReadOnlyList<Exception> Exceptions => BaseContainer.Exceptions;

        public AsyncEventHandler Refreshed { get; }

        private void RefreshAddBase(Func<object?, AsyncEventArgs, Task> handler)
        {
            BaseContainer.Refreshed.Add(handler);
        }
        private void RefreshRemoveBase(Func<object?, AsyncEventArgs, Task> handler)
        {
            BaseContainer.Refreshed.Add(handler);
        }

        public VirtualContainer(
            IContainer baseContainer,
            List<Func<IEnumerable<IContainer>, IEnumerable<IContainer>>> containerTransformators,
            List<Func<IEnumerable<IElement>, IEnumerable<IElement>>> elementTransformators,
            bool isPermanent = false,
            bool isTransitive = false,
            string? virtualContainerName = null)
        {
            Refreshed = new (RefreshAddBase, RefreshRemoveBase);
            BaseContainer = baseContainer;
            _containerTransformators = containerTransformators;
            _elementTransformators = elementTransformators;

            IsPermanent = isPermanent;
            IsTransitive = isTransitive;
            VirtualContainerName = virtualContainerName;
        }

        public async Task Init()
        {
            await InitItems();
        }

        private async Task InitItems()
        {
            Containers = _containerTransformators.Aggregate((await BaseContainer.GetContainers())?.AsEnumerable(), (a, t) => t(a!))?.ToList()?.AsReadOnly();
            Elements = _elementTransformators.Aggregate((await BaseContainer.GetElements())?.AsEnumerable(), (a, t) => t(a!))?.ToList()?.AsReadOnly();

            Items = (Elements != null
                    ? Containers?.Cast<IItem>().Concat(Elements)
                    : Containers?.Cast<IItem>())
                ?.ToList().AsReadOnly();
        }

        public async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false) => await BaseContainer.GetByPath(path, acceptDeepestMatch);

        public IContainer? GetParent() => BaseContainer.GetParent();

        public async Task Refresh()
        {
            await BaseContainer.Refresh();
            await InitItems();
        }

        public IContainer GetRealContainer() =>
            BaseContainer is VirtualContainer virtualContainer ? virtualContainer.GetRealContainer() : BaseContainer;

        public bool HasWithName(string name) =>
            VirtualContainerName == name
            || (BaseContainer is VirtualContainer virtualContainer
                && virtualContainer.HasWithName(name));

        public async Task<IContainer> ExceptWithName(string name)
        {
            if (BaseContainer is VirtualContainer virtualBaseContainer && virtualBaseContainer.VirtualContainerName == name)
            {
                var newContainer = new VirtualContainer(
                    await virtualBaseContainer.ExceptWithName(name),
                    _containerTransformators,
                    _elementTransformators,
                    IsPermanent,
                    IsTransitive,
                    VirtualContainerName);

                await newContainer.Init();
                return newContainer;
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

        public async Task<IContainer> CreateContainer(string name) => await BaseContainer.CreateContainer(name);
        public async Task<IElement> CreateElement(string name) => await BaseContainer.CreateElement(name);
        public async Task<bool> IsExists(string name) => await BaseContainer.IsExists(name);

        public Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            return Task.FromResult(Items);
        }
        public Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            return Task.FromResult(Containers);
        }
        public Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default)
        {
            return Task.FromResult(Elements);
        }

        public async Task Delete() => await BaseContainer.Delete();
        public async Task<IContainer> Clone()
        {
            return new VirtualContainer(
                await BaseContainer.Clone(),
                _containerTransformators,
                _elementTransformators,
                IsPermanent,
                IsTransitive,
                VirtualContainerName
            );
        }

        public async Task Rename(string newName) => await BaseContainer.Rename(newName);
        public async Task<bool> CanOpen() => await BaseContainer.CanOpen();
    }
}