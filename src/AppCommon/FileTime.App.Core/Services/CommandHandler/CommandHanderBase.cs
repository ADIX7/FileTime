using System.Reactive.Linq;
using FileTime.App.Core.Command;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services.CommandHandler
{
    public abstract class CommandHanderBase : ICommandHandler
    {
        private readonly Dictionary<Commands, Func<Task>> _commandHandlers = new();
        private readonly IAppState? _appState;

        protected CommandHanderBase(IAppState? appState = null)
        {
            _appState = appState;
        }

        public bool CanHandleCommand(Commands command) => _commandHandlers.ContainsKey(command);

        public async Task HandleCommandAsync(Commands command) => await _commandHandlers[command].Invoke();

        protected void AddCommandHandler(Commands command, Func<Task> handler) => _commandHandlers.Add(command, handler);
        protected void AddCommandHandlers(IEnumerable<(Commands command, Func<Task> handler)> commandHandlers)
        {
            foreach (var (command, handler) in commandHandlers)
            {
                AddCommandHandler(command, handler);
            }
        }

        protected void RemoveCommandHandler(Commands command) => _commandHandlers.Remove(command);

        protected IDisposable SaveSelectedTab(Action<ITabViewModel?> handler) => RunWithAppState(appState => appState.SelectedTab.Subscribe(handler));
        protected IDisposable SaveCurrentSelectedItem(Action<IItemViewModel?> handler)
            => RunWithAppState(appState => appState.SelectedTab.Select(t => t == null ? Observable.Return<IItemViewModel?>(null) : t.CurrentSelectedItem).Switch().Subscribe(handler));

        protected IDisposable SaveCurrentLocation(Action<IContainer?> handler)
            => RunWithAppState(appState => appState.SelectedTab.Select(t => t == null ? Observable.Return<IContainer?>(null) : t.CurrentLocation).Switch().Subscribe(handler));

        protected IDisposable SaveCurrentItems(Action<IEnumerable<IItemViewModel>> handler)
            => RunWithAppState(appState => appState.SelectedTab.Select(t => t?.CurrentItemsCollectionObservable ?? Observable.Return((IEnumerable<IItemViewModel>?)Enumerable.Empty<IItemViewModel>())).Switch().Subscribe(i => handler(i ?? Enumerable.Empty<IItemViewModel>())));

        private IDisposable RunWithAppState(Func<IAppState, IDisposable> act)
        {
            if (_appState == null) throw new NullReferenceException($"AppState is nit initialized in {nameof(CommandHanderBase)}.");

            return act(_appState);
        }
    }
}