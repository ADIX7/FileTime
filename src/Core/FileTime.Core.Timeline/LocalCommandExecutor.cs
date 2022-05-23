using FileTime.Core.Command;

namespace FileTime.Core.Timeline;

public class LocalCommandExecutor : ILocalCommandExecutor
{
    private readonly ICommandRunner _commandRunner;
    public event EventHandler<ICommand> CommandFinished;

    public LocalCommandExecutor(ICommandRunner commandRunner)
    {
        _commandRunner = commandRunner;
    }
    
    public void ExecuteCommand(ICommand command)
    {
        var context = new CommandRunnerContext(command);
        var thread = new Thread(new ParameterizedThreadStart(RunCommand));
        thread.Start(context);
    }

    private async void RunCommand(object? contextObj)
    {
        if (contextObj is not CommandRunnerContext context)
            throw new ArgumentException($"Parameter must be of type {typeof(CommandRunnerContext)}");
        try
        {
            await _commandRunner.RunCommandAsync(context.Command);
        }
        catch(Exception ex){}
        
        CommandFinished.Invoke(this, context.Command);
    }
}