using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using DeclarativeProperty;
using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.Timeline;
using FileTime.App.FrequencyNavigation.Services;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.GuiApp.App.CloudDrives;
using FileTime.GuiApp.App.InstanceManagement;
using FileTime.GuiApp.App.Services;
using FileTime.Providers.Local;
using FileTime.Providers.LocalAdmin;
using Microsoft.Extensions.Logging;
using MvvmGen;

namespace FileTime.GuiApp.App.ViewModels;

[ViewModel]
[Inject(typeof(IGuiAppState), "_appState")]
[Inject(typeof(ILocalContentProvider), "_localContentProvider")]
[Inject(typeof(IServiceProvider), PropertyName = "_serviceProvider")]
[Inject(typeof(ILogger<MainWindowViewModel>), PropertyName = "_logger")]
[Inject(typeof(IKeyInputHandlerService), PropertyName = "_keyInputHandlerService")]
[Inject(typeof(IUserCommandHandlerService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(ILifecycleService), PropertyName = "_lifecycleService")]
[Inject(typeof(IItemPreviewService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IDialogService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(ITimelessContentProvider), PropertyName = "_timelessContentProvider")]
[Inject(typeof(IFontService), "_fontService")]
[Inject(typeof(IFrequencyNavigationService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(ICommandPaletteService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IRefreshSmoothnessCalculator), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IAdminElevationManager), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IClipboardService), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IModalService), PropertyName = "_modalService")]
[Inject(typeof(ITimelineViewModel), PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IPossibleCommandsViewModel), PropertyName = "PossibleCommands", PropertyAccessModifier = AccessModifier.Public)]
[Inject(typeof(IInstanceMessageHandler), PropertyName = "_instanceMessageHandler")]
[Inject(typeof(ICloudDriveService), PropertyAccessModifier = AccessModifier.Public)]
public partial class MainWindowViewModel : IMainWindowViewModel
{
    public bool Loading => false;
    public IObservable<string?> MainFont => _fontService.MainFont.Select(x => x ?? "");
    public DeclarativeProperty<string> FatalError { get; } = new();
    public IReadOnlyList<WindowTransparencyLevel> TransparencyLevelHint { get; } = new[] {WindowTransparencyLevel.Blur};
    public IGuiAppState AppState => _appState;
    public DeclarativeProperty<string> Title { get; } = new();
    public Thickness IconStatusPanelMargin { get; private set; } = new(20, 10, 10, 10);
    public Action? FocusDefaultElement { get; set; }
    public Action? ShowWindow { get; set; }

    partial void OnInitialize()
    {
        _logger.LogInformation($"Starting {nameof(MainWindowViewModel)} initialization...");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            IconStatusPanelMargin = new(20, 10, 160, 10);
        }

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

        var title = "FileTime " + versionString;
#if DEBUG
        title += " (Debug)";
#endif

        Title.SetValueSafe(title);

        _modalService.AllModalClosed += (_, _) => FocusDefaultElement?.Invoke();
        _instanceMessageHandler.ShowWindow += () => ShowWindow?.Invoke();
        
        Task.Run(async () =>
        {
            await Task.Delay(100);
            await _lifecycleService.InitStartupHandlersAsync();
        });
    }

    public void ProcessKeyDown(KeyEventArgs e)
        => _keyInputHandlerService.ProcessKeyDown(e);

    public async Task OpenContainerByFullName(FullName fullName)
    {
        var resolvedItem = await _timelessContentProvider.GetItemByFullNameAsync(fullName, PointInTime.Present);
        if (resolvedItem is not IContainer resolvedContainer) return;
        await UserCommandHandlerService.HandleCommandAsync(
            new OpenContainerCommand(new AbsolutePath(_timelessContentProvider, resolvedContainer)));
    }

    public async Task RunOrOpenItem(IItemViewModel itemViewModel) =>
        await UserCommandHandlerService.HandleCommandAsync(
            new RunOrOpenCommand
            {
                Item = itemViewModel
            });

    public async Task OnExit()
        => await _lifecycleService.ExitAsync();
}