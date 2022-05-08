using System;
using System.Reflection;
using Avalonia.Input;
using FileTime.App.Core;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Services;
using FileTime.GuiApp.Services;
using FileTime.Providers.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvvmGen;

namespace FileTime.GuiApp.ViewModels;

[ViewModel]
[Inject(typeof(IGuiAppState), "_appState")]
[Inject(typeof(ILocalContentProvider), "_localContentProvider")]
[Inject(typeof(IServiceProvider), PropertyName = "_serviceProvider")]
[Inject(typeof(ILogger<MainWindowViewModel>), PropertyName = "_logger")]
[Inject(typeof(IKeyInputHandlerService), PropertyName = "_keyInputHandlerService")]
public partial class MainWindowViewModel : IMainWindowViewModelBase
{
    public bool Loading => false;
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

        //TODO: refactor
        if (AppState.Tabs.Count == 0)
        {
            var tab = _serviceProvider.GetInitableResolver<IContainer>(_localContentProvider).GetRequiredService<ITab>();
            var tabViewModel = _serviceProvider.GetInitableResolver(tab, 1).GetRequiredService<ITabViewModel>();

            _appState.AddTab(tabViewModel);
        }
    }

    public void ProcessKeyDown(Key key, KeyModifiers keyModifiers, Action<bool> setHandled)
    {
        _keyInputHandlerService.ProcessKeyDown(key, keyModifiers, setHandled);
    }
}