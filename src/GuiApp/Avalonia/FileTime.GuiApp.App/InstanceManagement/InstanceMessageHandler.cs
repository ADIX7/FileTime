using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.GuiApp.App.InstanceManagement.Messages;

namespace FileTime.GuiApp.App.InstanceManagement;

public class InstanceMessageHandler : IInstanceMessageHandler
{
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly ITimelessContentProvider _timelessContentProvider;

    public event Action? ShowWindow;

    public InstanceMessageHandler(
        IUserCommandHandlerService userCommandHandlerService,
        ITimelessContentProvider timelessContentProvider
    )
    {
        _userCommandHandlerService = userCommandHandlerService;
        _timelessContentProvider = timelessContentProvider;
    }

    public async Task HandleMessageAsync(IInstanceMessage message)
    {
        if (message is OpenContainers openContainers)
        {
            foreach (var container in openContainers.Containers)
            {
                var fullName = await _timelessContentProvider.GetFullNameByNativePathAsync(new NativePath(container));

                if (fullName is null) continue;

                await _userCommandHandlerService.HandleCommandAsync(new NewTabCommand(fullName));
            }
            
            ShowWindow?.Invoke();
        }
    }
}