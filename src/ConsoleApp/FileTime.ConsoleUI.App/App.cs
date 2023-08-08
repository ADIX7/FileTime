using FileTime.App.Core.Services;
using FileTime.ConsoleUI.App.KeyInputHandling;
using TerminalUI;

namespace FileTime.ConsoleUI.App;

public class App : IApplication
{
    private readonly ILifecycleService _lifecycleService;
    private readonly IConsoleAppState _consoleAppState;
    //private readonly IAppKeyService<Key> _appKeyService;
    private readonly MainWindow _mainWindow;
    private readonly IApplicationContext _applicationContext;
    private readonly IKeyInputHandlerService _keyInputHandlerService;

    public App(
        ILifecycleService lifecycleService,
        IKeyInputHandlerService keyInputHandlerService,
        IConsoleAppState consoleAppState,
        //IAppKeyService<Key> appKeyService,
        MainWindow mainWindow,
        IApplicationContext applicationContext)
    {
        _lifecycleService = lifecycleService;
        _keyInputHandlerService = keyInputHandlerService;
        _consoleAppState = consoleAppState;
        //_appKeyService = appKeyService;
        _mainWindow = mainWindow;
        _applicationContext = applicationContext;
    }

    public void Run()
    {
        Task.Run(async () => await _lifecycleService.InitStartupHandlersAsync()).Wait();

        _mainWindow.Initialize();
        
        _applicationContext.EventLoop.Run();
    }
}