using FileTime.Core.Models;

namespace FileTime.Core.Timeline;

public interface ICommandSchedulerNotifier
{
    Task RefreshContainer(FullName container);
}