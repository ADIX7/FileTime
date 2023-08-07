using FileTime.App.Core.Services;
using FileTime.ConsoleUI.App.Extensions;
using FileTime.ConsoleUI.App.KeyInputHandling;
using FileTime.Core.Models;
using Terminal.Gui;

namespace FileTime.ConsoleUI.App;

public class App : IApplication
{
    private readonly ILifecycleService _lifecycleService;
    private readonly IConsoleAppState _consoleAppState;
    private readonly IAppKeyService<Key> _appKeyService;
    private readonly MainWindow _mainWindow;
    private readonly IKeyInputHandlerService _keyInputHandlerService = null!;

    public App(
        ILifecycleService lifecycleService,
        IConsoleAppState consoleAppState,
        IAppKeyService<Key> appKeyService,
        MainWindow mainWindow)
    {
        _lifecycleService = lifecycleService;
        _consoleAppState = consoleAppState;
        _appKeyService = appKeyService;
        _mainWindow = mainWindow;
    }

    public void Run()
    {
        Task.Run(async () => await _lifecycleService.InitStartupHandlersAsync()).Wait();

        _mainWindow.Initialize();

        Application.Init();

        foreach (var element in _mainWindow.GetElements())
        {
            Application.Top.Add(element);
        }

        Application.RootKeyEvent += e =>
        {
            if (e.ToGeneralKeyEventArgs(_appKeyService) is { } args)
            {
                _keyInputHandlerService.HandleKeyInput(args);

                return args.Handled;
            }

            return false;
        };

        Application.Run();
        Application.Shutdown();
    }
}