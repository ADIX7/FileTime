using FileTime.Core.Models;

namespace FileTime.Core.Providers
{
    public interface IContentProvider : IContainer
    {
        bool SupportsContentStreams { get; }
        string Protocol { get; }

        Task<bool> CanHandlePath(string path);

        void SetParent(IContainer container);
    }
}