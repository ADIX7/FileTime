using System.Collections.Specialized;
using System.ComponentModel;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Configuration;
using FileTime.ConsoleUI.App.KeyInputHandling;
using FileTime.ConsoleUI.App.Styling;
using GeneralInputKey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TerminalUI;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Models;

namespace FileTime.ConsoleUI.App;

public class App : IApplication
{
    private static readonly List<Keys> KeysToFurtherProcess = new()
    {
        Keys.Enter,
        Keys.Escape
    };

    private readonly ILifecycleService _lifecycleService;

    private readonly IAppKeyService<ConsoleKey> _appKeyService;
    private readonly MainWindow _mainWindow;
    private readonly IApplicationContext _applicationContext;
    private readonly IConsoleDriver _consoleDriver;
    private readonly IAppState _appState;
    private readonly IThemeProvider _themeProvider;
    private readonly ILogger<App> _logger;
    private readonly IKeyInputHandlerService _keyInputHandlerService;
    private readonly Thread _renderThread;

    public App(
        ILifecycleService lifecycleService,
        IKeyInputHandlerService keyInputHandlerService,
        IAppKeyService<ConsoleKey> appKeyService,
        MainWindow mainWindow,
        IApplicationContext applicationContext,
        IConsoleDriver consoleDriver,
        IAppState appState,
        IThemeProvider themeProvider,
        IOptions<ConsoleApplicationConfiguration> consoleApplicationConfiguration,
        ILogger<App> logger)
    {
        _lifecycleService = lifecycleService;
        _keyInputHandlerService = keyInputHandlerService;
        _appKeyService = appKeyService;
        _mainWindow = mainWindow;
        _applicationContext = applicationContext;
        _consoleDriver = consoleDriver;
        _appState = appState;
        _themeProvider = themeProvider;
        _logger = logger;
        
        if (themeProvider is INotifyPropertyChanged notifyPropertyChanged)
            notifyPropertyChanged.PropertyChanged += ThemeProviderPropertyChanged;
        
        _applicationContext.Theme = themeProvider.CurrentTheme.ConsoleTheme ?? _applicationContext.Theme;

        _applicationContext.SupportUtf8Output = !consoleApplicationConfiguration.Value.DisableUtf8;

        _renderThread = new Thread(Render);
    }

    private void ThemeProviderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IThemeProvider.CurrentTheme))
        {
            UpdateConsoleTheme();
        }
    }

    private void UpdateConsoleTheme() 
        => _applicationContext.Theme = _themeProvider.CurrentTheme.ConsoleTheme;

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
        
        _consoleDriver.ExitRestrictedMode();
        _consoleDriver.Clear();
        var size = _consoleDriver.GetWindowSize();
        var shutdownText = "Shutting down...";
        
        _consoleDriver.SetCursorPosition(new Position(size.Width / 2 - shutdownText.Length / 2, size.Height / 2));
        _consoleDriver.Write(shutdownText);

        Task.Run(async () => await _lifecycleService.ExitAsync()).Wait();
    }

    private void Render() => _applicationContext.RenderEngine.Run();
}