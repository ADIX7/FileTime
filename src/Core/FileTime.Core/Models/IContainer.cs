using AsyncEvent;

namespace FileTime.Core.Models
{
    public interface IContainer : IItem
    {
        IReadOnlyList<Exception> Exceptions { get; }
        Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default);
        Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default);
        Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default);
        bool AllowRecursiveDeletion { get; }

        Task RefreshAsync(CancellationToken token = default);
        public async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            if (path == null) return this;
            var paths = path.Split(Constants.SeparatorChar);

            var item = (await GetItems())?.FirstOrDefault(i => i.Name == paths[0]);

            if (paths.Length == 1)
            {
                return item;
            }

            if (item is IContainer container)
            {
                IItem? result = null;
                try
                {
                    result = await container.GetByPath(string.Join(Constants.SeparatorChar, paths.Skip(1)), acceptDeepestMatch);
                }
                catch { }
                return result == null && acceptDeepestMatch ? this : result;
            }

            return null;
        }

        Task<IContainer> CreateContainerAsync(string name);
        Task<IElement> CreateElementAsync(string name);

        Task<bool> IsExistsAsync(string name);

        Task<IContainer> CloneAsync();
        Task<bool> CanOpenAsync();
        void Unload();

        bool IsLoaded { get; }
        bool SupportsDirectoryLevelSoftDelete { get; }

        AsyncEventHandler Refreshed { get; }
    }
}