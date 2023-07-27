using System.Reactive.Linq;
using System.Reflection;
using Avalonia.Input;
using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;
using FileTime.App.FrequencyNavigation.Services;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.GuiApp.Services;
using FileTime.Providers.Local;
using FileTime.Providers.LocalAdmin;
using Microsoft.Extensions.Logging;
using MvvmGen;

namespace FileTime.GuiApp.ViewModels;

[ViewModel]
[Inject(typeof(IGuiAppState), "_appState")]
[Inject(typeof(ILocalContentProvider), "_localContentProvider")]
[Inject(typeof(IServiceProvider), PropertyName = "_serviceProvider")]
[Inject(typeof(ILogger<MainWindowViewModel>), PropertyName = "_logger")]
[Inject(typeof(IKeyInputHandlerService), PropertyName = "_keyInputHandlerService")]
[Inject(typeof(IUserCommandHandlerService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(LifecycleService), PropertyName = "_lifecycleService")]
[Inject(typeof(IItemPreviewService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IDialogService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(ITimelessContentProvider), PropertyName = "_timelessContentProvider")]
[Inject(typeof(IFontService), "_fontService")]
[Inject(typeof(IFrequencyNavigationService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(ICommandPaletteService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IRefreshSmoothnessCalculator), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IAdminElevationManager), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IClipboardService), PropertyAccessModifier = AccessModifier.Public)]
public partial class MainWindowViewModel : IMainWindowViewModel
{
    public bool Loading => false;
    public IObservable<string?> MainFont => _fontService.MainFont.Select(x => x ?? "");
    public IGuiAppState AppState => _appState;
    public string Title { get; private set; }

    partial void OnInitialize()
    {
        _logger?.LogInformation($"Starting {nameof(MainWindowViewModel)} initialization...");

        var version = Assembly.GetEntryAssembly()!.GetName().Version;
        var versionString = "Unknown version";
        if (version != null)
        {
            versionString = $"{version.Major}.{version.Minor}.{version.Build}";
            if (version.Revision != 0)
            {
                versionString += $" ({version.Revision})";
            }
        }

        Title = "FileTime " + versionString;

        Task.Run(async () => await _lifecycleService.InitStartupHandlersAsync()).Wait();
    }

    public void ProcessKeyDown(Key key, KeyModifiers keyModifiers, Action<bool> setHandled)
    {
        _keyInputHandlerService.ProcessKeyDown(key, keyModifiers, setHandled);
    }

    public async Task OpenContainerByFullName(FullName fullName)
    {
        var resolvedItem = await _timelessContentProvider.GetItemByFullNameAsync(fullName, PointInTime.Present);
        if (resolvedItem is not IContainer resolvedContainer) return;
        await UserCommandHandlerService.HandleCommandAsync(
            new OpenContainerCommand(new AbsolutePath(_timelessContentProvider, resolvedContainer)));
    }

    public async Task OnExit()
    {
        await _lifecycleService.ExitAsync();
    }
}