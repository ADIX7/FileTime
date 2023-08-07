using FileTime.App.Core.Services;
using FileTime.ConsoleUI.App.KeyInputHandling;

namespace FileTime.ConsoleUI.App;

public class App : IApplication
{
    private readonly ILifecycleService _lifecycleService;
    private readonly IConsoleAppState _consoleAppState;
    //private readonly IAppKeyService<Key> _appKeyService;
    private readonly MainWindow _mainWindow;
    private readonly IKeyInputHandlerService _keyInputHandlerService;

    public App(
        ILifecycleService lifecycleService,
        IKeyInputHandlerService keyInputHandlerService,
        IConsoleAppState consoleAppState,
        //IAppKeyService<Key> appKeyService,
        MainWindow mainWindow)
    {
        _lifecycleService = lifecycleService;
        _keyInputHandlerService = keyInputHandlerService;
        _consoleAppState = consoleAppState;
        //_appKeyService = appKeyService;
        _mainWindow = mainWindow;
    }

    public void Run()
    {
        Console.WriteLine("Loading...");
        Task.Run(async () => await _lifecycleService.InitStartupHandlersAsync()).Wait();

        _mainWindow.Initialize();
    }
}