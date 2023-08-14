using System.Collections.Specialized;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.KeyInputHandling;
using GeneralInputKey;
using TerminalUI;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Traits;

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
        IAppState appState)
    {
        _lifecycleService = lifecycleService;
        _keyInputHandlerService = keyInputHandlerService;
        _consoleAppState = consoleAppState;
        _appKeyService = appKeyService;
        _mainWindow = mainWindow;
        _applicationContext = applicationContext;
        _consoleDriver = consoleDriver;
        _appState = appState;

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

        while (_applicationContext.IsRunning)
        {
            if (_consoleDriver.CanRead())
            {
                var key = _consoleDriver.ReadKey();

                if (_appKeyService.MapKey(key.Key) is { } mappedKey)
                {
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

                    if (focused is null || (!keyEventArgs.Handled && KeysToFurtherProcess.Contains(keyEventArgs.Key)))
                    {
                        _keyInputHandlerService.HandleKeyInput(keyEventArgs, specialKeysStatus);
                    }
                }
            }

            Thread.Sleep(10);
        }
    }

    private void Render() => _applicationContext.RenderEngine.Run();
}