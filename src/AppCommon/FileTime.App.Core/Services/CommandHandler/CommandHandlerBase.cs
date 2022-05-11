using System.Reactive.Linq;
using DynamicData;
using FileTime.App.Core.Command;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services.CommandHandler;

public abstract class CommandHandlerBase : ICommandHandler
{
    private readonly Dictionary<Command.Command, Func<Task>> _commandHandlers = new();
    private readonly IAppState? _appState;

    protected CommandHandlerBase(IAppState? appState = null)
    {
        _appState = appState;
    }

    public bool CanHandleCommand(Command.Command command) => _commandHandlers.ContainsKey(command);

    public async Task HandleCommandAsync(Command.Command command) => await _commandHandlers[command].Invoke();

    protected void AddCommandHandler(Command.Command command, Func<Task> handler) => _commandHandlers.Add(command, handler);
    protected void AddCommandHandlers(IEnumerable<(Command.Command command, Func<Task> handler)> commandHandlers)
    {
        foreach (var (command, handler) in commandHandlers)
        {
            AddCommandHandler(command, handler);
        }
    }

    protected void RemoveCommandHandler(Command.Command command) => _commandHandlers.Remove(command);

    protected IDisposable SaveSelectedTab(Action<ITabViewModel?> handler) => RunWithAppState(appState => appState.SelectedTab.Subscribe(handler));
    protected IDisposable SaveCurrentSelectedItem(Action<IItemViewModel?> handler)
        => RunWithAppState(appState => appState.SelectedTab.Select(t => t == null ? Observable.Return<IItemViewModel?>(null) : t.CurrentSelectedItem).Switch().Subscribe(handler));

    protected IDisposable SaveCurrentLocation(Action<IContainer?> handler)
        => RunWithAppState(appState => appState.SelectedTab.Select(t => t == null ? Observable.Return<IContainer?>(null) : t.CurrentLocation).Switch().Subscribe(handler));

    protected IDisposable SaveCurrentItems(Action<IEnumerable<IItemViewModel>> handler)
        => RunWithAppState(appState => appState.SelectedTab.Select(t => t?.CurrentItemsCollectionObservable ?? Observable.Return((IEnumerable<IItemViewModel>?)Enumerable.Empty<IItemViewModel>())).Switch().Subscribe(i => handler(i ?? Enumerable.Empty<IItemViewModel>())));

    protected IDisposable SaveMarkedItems(Action<IChangeSet<IAbsolutePath>> handler)
        => RunWithAppState(appstate => appstate.SelectedTab.Select(t => t == null ? Observable.Empty<IChangeSet<IAbsolutePath>>() : t.MarkedItems).Switch().Subscribe(handler));

    private IDisposable RunWithAppState(Func<IAppState, IDisposable> act)
    {
        if (_appState == null) throw new NullReferenceException($"AppState is nit initialized in {nameof(CommandHandlerBase)}.");

        return act(_appState);
    }
}