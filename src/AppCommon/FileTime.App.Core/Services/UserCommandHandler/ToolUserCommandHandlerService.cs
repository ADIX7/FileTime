using System.Diagnostics;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class ToolUserCommandHandlerService : UserCommandHandlerServiceBase
{
    private readonly ISystemClipboardService _systemClipboardService;
    private IContainer? _currentLocation;
    private IItemViewModel? _currentSelectedItem;

    public ToolUserCommandHandlerService(IAppState appState, ISystemClipboardService systemClipboardService) : base(appState)
    {
        _systemClipboardService = systemClipboardService;
        SaveCurrentLocation(l => _currentLocation = l);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);

        AddCommandHandlers(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<OpenInDefaultFileExplorerCommand>(OpenInDefaultFileExplorer),
            new TypeUserCommandHandler<CopyNativePathCommand>(CopyNativePath),
        });
    }

    private async Task CopyNativePath()
    {
        if (_currentSelectedItem?.BaseItem?.NativePath is null) return;
        await _systemClipboardService.CopyToClipboardAsync(_currentSelectedItem.BaseItem.NativePath.Path);
    }

    private Task OpenInDefaultFileExplorer()
    {
        if (_currentLocation?.NativePath is null) return Task.CompletedTask;
        Process.Start("explorer.exe", "\"" + _currentLocation.NativePath.Path + "\"");
        return Task.CompletedTask;
    }
}