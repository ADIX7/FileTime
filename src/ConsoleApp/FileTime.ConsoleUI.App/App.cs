using System.Collections.Specialized;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.KeyInputHandling;
using FileTime.Core.Command.CreateContainer;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using GeneralInputKey;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TerminalUI;
using TerminalUI.ConsoleDrivers;

namespace FileTime.ConsoleUI.App;

public class App : IApplication
{
    private static readonly List<Keys> KeysToFurtherProcess = new()
    {
        Keys.Enter,
        Keys.Escape
    };

    private readonly ILifecycleService _lifecycleService;

    private readonly IConsoleAppState _consoleAppState;

    private readonly IAppKeyService<ConsoleKey> _appKeyService;
    private readonly MainWindow _mainWindow;
    private readonly IApplicationContext _applicationContext;
    private readonly IConsoleDriver _consoleDriver;
    private readonly IAppState _appState;
    private readonly ILogger<App> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IKeyInputHandlerService _keyInputHandlerService;
    private readonly Thread _renderThread;

    public App(
        ILifecycleService lifecycleService,
        IKeyInputHandlerService keyInputHandlerService,
        IConsoleAppState consoleAppState,
        IAppKeyService<ConsoleKey> appKeyService,
        MainWindow mainWindow,
        IApplicationContext applicationContext,
        IConsoleDriver consoleDriver,
        IAppState appState,
        ILogger<App> logger,
        IServiceProvider serviceProvider)
    {
        _lifecycleService = lifecycleService;
        _keyInputHandlerService = keyInputHandlerService;
        _consoleAppState = consoleAppState;
        _appKeyService = appKeyService;
        _mainWindow = mainWindow;
        _applicationContext = applicationContext;
        _consoleDriver = consoleDriver;
        _appState = appState;
        _logger = logger;
        _serviceProvider = serviceProvider;

        _renderThread = new Thread(Render);
    }

    public void Run()
    {
        Task.Run(async () => await _lifecycleService.InitStartupHandlersAsync()).Wait();

        ((INotifyCollectionChanged) _appState.Tabs).CollectionChanged += (_, _) =>
        {
            if (_appState.Tabs.Count == 0)
                _applicationContext.IsRunning = false;
        };

        foreach (var rootView in _mainWindow.RootViews())
        {
            _applicationContext.RenderEngine.AddViewToPermanentRenderGroup(rootView);
        }

        _applicationContext.IsRunning = true;
        _renderThread.Start();

        var focusManager = _applicationContext.FocusManager;

        var command = _serviceProvider.GetRequiredService<CreateContainerCommand>();
        command.Init(new FullName("local/C:/Test3"), "container1");
        var scheduler = _serviceProvider.GetRequiredService<ICommandScheduler>();

        scheduler.AddCommand(command);

        while (_applicationContext.IsRunning)
        {
            try
            {
                if (_consoleDriver.CanRead())
                {
                    var key = _consoleDriver.ReadKey();

                    var mappedKey = _appKeyService.MapKey(key.Key);
                    SpecialKeysStatus specialKeysStatus = new(
                        (key.Modifiers & ConsoleModifiers.Alt) != 0,
                        (key.Modifiers & ConsoleModifiers.Shift) != 0,
                        (key.Modifiers & ConsoleModifiers.Control) != 0
                    );

                    var keyEventArgs = new GeneralKeyEventArgs
                    {
                        Key = mappedKey,
                        KeyChar = key.KeyChar,
                        SpecialKeysStatus = specialKeysStatus
                    };

                    var focused = focusManager.Focused;
                    if (focused is { })
                    {
                        focused.HandleKeyInput(keyEventArgs);
                        _applicationContext.FocusManager.HandleKeyInput(keyEventArgs);
                    }

                    if (focused is null || (keyEventArgs is {Handled: false, Key: { } k} && KeysToFurtherProcess.Contains(k)))
                    {
                        _keyInputHandlerService.HandleKeyInput(keyEventArgs, specialKeysStatus);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while handling key input");
            }

            Thread.Sleep(10);
        }
    }

    private void Render() => _applicationContext.RenderEngine.Run();
}