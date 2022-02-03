using AsyncEvent;

namespace FileTime.Core.Models
{
    public interface IContainer : IItem
    {
        IReadOnlyList<Exception> Exceptions { get; }
        Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default);
        Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default);
        Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default);

        Task RefreshAsync(CancellationToken token = default);
        Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false);
        Task<IContainer> CreateContainer(string name);
        Task<IElement> CreateElement(string name);

        Task<bool> IsExists(string name);

        Task<IContainer> Clone();
        Task<bool> CanOpen();

        bool IsLoaded { get; }
        bool SupportsDirectoryLevelSoftDelete { get; }

        AsyncEventHandler Refreshed { get; }
    }
}