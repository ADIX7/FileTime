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
        async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            var paths = path.Split(Constants.SeparatorChar);

            var item = (await GetItems())?.FirstOrDefault(i => i.Name == paths[0]);

            if (paths.Length == 1)
            {
                return item;
            }

            if (item is IContainer container)
            {
                var result = await container.GetByPath(string.Join(Constants.SeparatorChar, paths.Skip(1)), acceptDeepestMatch);
                return result == null && acceptDeepestMatch ? this : result;
            }

            return null;
        }

        Task<IContainer> CreateContainer(string name);
        Task<IElement> CreateElement(string name);

        Task<bool> IsExists(string name);

        Task<IContainer> Clone();
        Task<bool> CanOpen();
        void Unload();

        bool IsLoaded { get; }
        bool SupportsDirectoryLevelSoftDelete { get; }

        AsyncEventHandler Refreshed { get; }
    }
}