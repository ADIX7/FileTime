using FileTime.GuiApp.App.InstanceManagement.Messages;

namespace FileTime.GuiApp.App.InstanceManagement;

public class DummyInstanceMessageHandler : IInstanceMessageHandler
{
    public Task HandleMessageAsync(IInstanceMessage message) => throw new NotImplementedException();
    public event Action? ShowWindow;
}