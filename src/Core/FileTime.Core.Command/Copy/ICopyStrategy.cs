using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Command.Copy;

public interface ICopyStrategy
{
    Task CreateContainerAsync(IContainer target, string name, PointInTime currentTime);
    Task ContainerCopyDoneAsync(AbsolutePath containerPath);
    Task CopyAsync(AbsolutePath from, AbsolutePath to, CopyCommandContext context);
}