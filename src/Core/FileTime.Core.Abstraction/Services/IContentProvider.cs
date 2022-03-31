using FileTime.Core.Behaviors;
using FileTime.Core.Models;

namespace FileTime.Core.Services
{
    public interface IContentProvider : IContainer, IOnContainerEnter
    {
        Task<IItem> GetItemByFullNameAsync(FullName fullName);
        Task<IItem> GetItemByNativePathAsync(NativePath nativePath);
        Task<List<IAbsolutePath>> GetItemsByContainerAsync(FullName fullName);
    }
}