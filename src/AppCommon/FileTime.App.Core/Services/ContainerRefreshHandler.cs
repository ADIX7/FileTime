using System.Reactive.Linq;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.Services;

public class ContainerRefreshHandler : IExitHandler
{
    private readonly List<IDisposable> _refreshSubscriptions = new();
    private List<FullName> _folders = new();

    public ContainerRefreshHandler(
        ICommandScheduler commandScheduler,
        IUserCommandHandlerService userCommandHandlerService,
        IAppState appState
    )
    {
        _refreshSubscriptions.Add(
            Observable.CombineLatest(
                appState.SelectedTab.Select(t => t?.CurrentLocation.Select(l => l?.FullName) ?? Observable.Never<FullName>()).Switch(),
                appState.SelectedTab.Select(t => t?.CurrentLocation.Select(l => l?.Parent?.Path) ?? Observable.Never<FullName>()).Switch(),
                appState.SelectedTab.Select(t => t?.CurrentSelectedItem.Select(l => l?.BaseItem?.FullName) ?? Observable.Never<FullName>()).Switch(),
                (a, b, c) => new[] {a, b, c}
            ).Subscribe(folders => { _folders = folders.Where(f => f is not null).Cast<FullName>().ToList(); })
        );


        _refreshSubscriptions.Add(
            commandScheduler.ContainerToRefresh.Subscribe(refreshRequestedIn =>
            {
                if (_folders.Contains(refreshRequestedIn))
                {
                    userCommandHandlerService.HandleCommandAsync(RefreshCommand.Instance);
                }
            })
        );
    }

    public Task ExitAsync(CancellationToken token = default)
    {
        foreach (var refreshSubscription in _refreshSubscriptions)
        {
            refreshSubscription.Dispose();
        }

        return Task.CompletedTask;
    }
}