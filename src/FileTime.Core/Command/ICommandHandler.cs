namespace FileTime.Core.Command
{
    public interface ICommandHandler
    {
        bool CanHandle(object command);
        void Execute(object command);
    }
}