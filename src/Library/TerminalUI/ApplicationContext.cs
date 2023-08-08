namespace TerminalUI;

public class ApplicationContext : IApplicationContext
{
    public IEventLoop EventLoop { get; init; }
    public bool IsRunning { get; set; }

    public ApplicationContext()
    {
        EventLoop = new EventLoop(this);
    }
}