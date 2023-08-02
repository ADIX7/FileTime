using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DeclarativeProperty;
using DynamicData;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.Services.UserCommandHandler;

public abstract class UserCommandHandlerServiceBase : AggregatedUserCommandHandler
{
    private readonly IAppState? _appState;
    private readonly ITimelessContentProvider? _timelessContentProvider;

    protected UserCommandHandlerServiceBase(
        IAppState? appState = null,
        ITimelessContentProvider? timelessContentProvider = null)
    {
        _appState = appState;
        _timelessContentProvider = timelessContentProvider;
    }

    protected IDisposable SaveSelectedTab(Action<ITabViewModel?> handler) => RunWithAppState(appState => appState.SelectedTab.Subscribe(handler));

    protected IDisposable SaveCurrentSelectedItem(Action<IDeclarativeProperty<IItemViewModel?>?> handler)
        => RunWithAppState(appState => appState.SelectedTab.Select(t => t?.CurrentSelectedItem).Subscribe(handler));

    protected IDisposable SaveCurrentLocation(Action<IDeclarativeProperty<IContainer?>?> handler)
        => RunWithAppState(appState => appState.SelectedTab.Select(t => t?.CurrentLocation).Subscribe(handler));

    protected IDisposable SaveCurrentItems(Action<IDeclarativeProperty<ObservableCollection<IItemViewModel>?>?> handler)
        => RunWithAppState(appState => appState.SelectedTab.Select(t => t?.CurrentItems).Subscribe(handler));
    
    protected IDisposable SaveCurrentPointInTime(Action<PointInTime> handler)
        => RunWithTimelessContentProvider(timelessContentProvider => timelessContentProvider.CurrentPointInTime.Subscribe(handler));

    private IDisposable RunWithAppState(Func<IAppState, IDisposable> act)
    {
        if (_appState == null) throw new NullReferenceException($"AppState is not initialized in {nameof(UserCommandHandlerServiceBase)}.");

        return act(_appState);
    }

    private IDisposable RunWithTimelessContentProvider(Func<ITimelessContentProvider, IDisposable> act)
    {
        if (_timelessContentProvider == null) throw new NullReferenceException($"TimelessContainer is not initialized in {nameof(UserCommandHandlerServiceBase)}.");

        return act(_timelessContentProvider);
    }
}