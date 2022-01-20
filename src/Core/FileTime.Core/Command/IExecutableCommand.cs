namespace FileTime.Core.Command
{
    public interface IExecutableCommand : ICommand
    {
        Task Execute();
    }
}