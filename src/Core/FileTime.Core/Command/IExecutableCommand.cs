namespace FileTime.Core.Command
{
    public interface IExecutableCommand : ICommand
    {
        void Execute();
    }
}