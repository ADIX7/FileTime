using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.Timeline
{
    public class TimeProvider : IContentProvider
    {
        private readonly PointInTime _pointInTime;

        public bool IsLoaded => true;

        public AsyncEventHandler Refreshed { get; } = new();

        public string Name => "time";

        public string? FullName => null;

        public bool IsHidden => false;

        public SupportsDelete CanDelete => SupportsDelete.False;

        public bool CanRename => false;

        public IContentProvider Provider => this;

        public IReadOnlyList<Exception> Exceptions { get; } = new List<Exception>().AsReadOnly();

        public bool SupportsDirectoryLevelSoftDelete => false;

        public bool IsDestroyed => false;

        public TimeProvider(PointInTime pointInTime)
        {
            _pointInTime = pointInTime;
        }

        public bool CanHandlePath(string path)
        {
            throw new NotImplementedException();
        }

        public Task<IContainer> Clone() => Task.FromResult((IContainer)this);

        public Task<IContainer> CreateContainer(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IElement> CreateElement(string name)
        {
            throw new NotImplementedException();
        }

        public Task Delete(bool hardDelete = false) => throw new NotSupportedException();

        public Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IContainer? GetParent() => null;

        public Task<IReadOnlyList<IContainer>> GetRootContainers(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsExists(string name)
        {
            throw new NotImplementedException();
        }

        public Task RefreshAsync(CancellationToken token = default) => Task.CompletedTask;

        public Task Rename(string newName) => throw new NotSupportedException();

        public void SetParent(IContainer container) { }
        public Task<bool> CanOpen() => Task.FromResult(true);

        public void Destroy() { }

        public void Unload() { }
    }
}