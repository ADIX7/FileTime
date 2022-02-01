using FileTime.Core.Timeline;

namespace FileTime.Core.Command
{
    public interface ICommandHandler
    {
        bool CanHandle(object command);
        Task ExecuteAsync(object command, TimeRunner timeRunner);
    }
}