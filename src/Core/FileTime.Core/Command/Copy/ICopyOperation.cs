using FileTime.Core.Models;

namespace FileTime.Core.Command.Copy
{
    public interface ICopyOperation
    {
        Task ContainerCopyDoneAsync(AbsolutePath path);
        Task CopyAsync(AbsolutePath from, AbsolutePath to, OperationProgress? operation, CopyCommandContext context);
        Task CreateContainerAsync(IContainer target, string name);
    }
}