using FileTime.App.Core.Services;
using FileTime.GuiApp.App.InstanceManagement.Messages;

namespace FileTime.GuiApp.App.InstanceManagement;

public interface IInstanceManager : IStartupHandler, IExitHandler
{
    Task SendMessageAsync<T>(T message, CancellationToken token = default) where T : class, IInstanceMessage;
    Task<bool> TryConnectAsync(CancellationToken token = default);
}