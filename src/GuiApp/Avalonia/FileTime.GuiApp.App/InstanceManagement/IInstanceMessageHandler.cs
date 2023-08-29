using FileTime.GuiApp.App.InstanceManagement.Messages;

namespace FileTime.GuiApp.App.InstanceManagement;

public interface IInstanceMessageHandler
{
    Task HandleMessageAsync(IInstanceMessage message);
    event Action? ShowWindow;
}