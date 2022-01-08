using FileTime.Core.Models;

namespace FileTime.Core.Providers
{
    public interface IContentProvider : IContainer
    {
        IReadOnlyList<IContainer> RootContainers { get; }

        bool CanHandlePath(string path);

        void SetParent(IContainer container);
    }
}