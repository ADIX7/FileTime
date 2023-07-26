namespace FileTime.Server.Common;

public class ApplicationStopper : IApplicationStopper
{
    private readonly Action _stopAction;

    public ApplicationStopper(Action stopAction)
    {
        ArgumentNullException.ThrowIfNull(stopAction);
        _stopAction = stopAction;
    }


    public void Stop() => _stopAction();
}