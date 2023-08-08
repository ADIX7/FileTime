using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.ConsoleUI.App.KeyInputHandling;
using TerminalUI;
using TerminalUI.ConsoleDrivers;

namespace FileTime.ConsoleUI.App;

public class App : IApplication
{
    private readonly ILifecycleService _lifecycleService;

    private readonly IConsoleAppState _consoleAppState;

    private readonly IAppKeyService<ConsoleKey> _appKeyService;
    private readonly MainWindow _mainWindow;
    private readonly IApplicationContext _applicationContext;
    private readonly IConsoleDriver _consoleDriver;
    private readonly IKeyInputHandlerService _keyInputHandlerService;
    private readonly Thread _renderThread;

    public App(
        ILifecycleService lifecycleService,
        IKeyInputHandlerService keyInputHandlerService,
        IConsoleAppState consoleAppState,
        IAppKeyService<ConsoleKey> appKeyService,
        MainWindow mainWindow,
        IApplicationContext applicationContext,
        IConsoleDriver consoleDriver)
    {
        _lifecycleService = lifecycleService;
        _keyInputHandlerService = keyInputHandlerService;
        _consoleAppState = consoleAppState;
        _appKeyService = appKeyService;
        _mainWindow = mainWindow;
        _applicationContext = applicationContext;
        _consoleDriver = consoleDriver;

        _renderThread = new Thread(Render);
    }

    public void Run()
    {
        Task.Run(async () => await _lifecycleService.InitStartupHandlersAsync()).Wait();

        _mainWindow.Initialize();
        foreach (var rootView in _mainWindow.RootViews())
        {
            _applicationContext.EventLoop.AddViewToRender(rootView);
        }

        _applicationContext.IsRunning = true;
        _renderThread.Start();

        while (_applicationContext.IsRunning)
        {
            if (_consoleDriver.CanRead())
            {
                var key = _consoleDriver.ReadKey();
                if (_appKeyService.MapKey(key.Key) is { } mappedKey)
                {
                    var keyEventArgs = new GeneralKeyEventArgs
                    {
                        Key = mappedKey
                    };
                    _keyInputHandlerService.HandleKeyInput(keyEventArgs);
                }
            }
            Thread.Sleep(10);
        }
    }

    private void Render() => _applicationContext.EventLoop.Run();
}