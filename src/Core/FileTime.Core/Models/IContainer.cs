using AsyncEvent;

namespace FileTime.Core.Models
{
    public interface IContainer : IItem
    {
        Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default);
        Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default);
        Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default);

        Task Refresh();
        IContainer? GetParent();
        Task<IItem?> GetByPath(string path);
        Task<IContainer> CreateContainer(string name);
        Task<IElement> CreateElement(string name);

        Task<bool> IsExists(string name);

        AsyncEventHandler Refreshed { get; }
    }
}