using System;
using Avalonia.Input;
using FileTime.App.Core;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Services;
using FileTime.Providers.Local;
using Microsoft.Extensions.DependencyInjection;
using MvvmGen;

namespace FileTime.GuiApp.ViewModels
{
    [ViewModel]
    [Inject(typeof(IAppState), "_appState")]
    [Inject(typeof(ILocalContentProvider), "_localContentProvider")]
    [Inject(typeof(IServiceProvider), PropertyName = "_serviceProvider")]
    public partial class MainWindowViewModel : IMainWindowViewModelBase
    {
        public bool Loading => false;
        public IAppState AppState => _appState;

        partial void OnInitialize()
        {
            if (AppState.Tabs.Count == 0)
            {
                var tab = _serviceProvider.GetInitableResolver<IContainer>(_localContentProvider).GetRequiredService<ITab>();
                var tabViewModel = _serviceProvider.GetInitableResolver(tab).GetRequiredService<ITabViewModel>();

                _appState.AddTab(tabViewModel);
            }
        }

        public void ProcessKeyDown(Key key, KeyModifiers keyModifiers, Action<bool> setHandled)
        {
        }
    }
}
