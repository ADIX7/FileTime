using System.Diagnostics;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.App.Search;
using FileTime.Core.Interactions;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class ToolUserCommandHandlerService : UserCommandHandlerServiceBase
{
    private readonly ISystemClipboardService _systemClipboardService;
    private readonly IUserCommunicationService _userCommunicationService;
    private readonly ISearchManager _searchManager;
    private readonly IItemNameConverterService _itemNameConverterService;
    private IContainer? _currentLocation;
    private IItemViewModel? _currentSelectedItem;

    public ToolUserCommandHandlerService(
        IAppState appState,
        ISystemClipboardService systemClipboardService,
        IUserCommunicationService userCommunicationService,
        ISearchManager searchManager,
        IItemNameConverterService itemNameConverterService) : base(appState)
    {
        _systemClipboardService = systemClipboardService;
        _userCommunicationService = userCommunicationService;
        _searchManager = searchManager;
        _itemNameConverterService = itemNameConverterService;
        SaveCurrentLocation(l => _currentLocation = l);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);

        AddCommandHandlers(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<OpenInDefaultFileExplorerCommand>(OpenInDefaultFileExplorer),
            new TypeUserCommandHandler<CopyNativePathCommand>(CopyNativePath),
            new TypeUserCommandHandler<SearchCommand>(Search),
        });
    }

    private async Task Search(SearchCommand searchCommand)
    {
        if(_currentLocation is null) return;
        
        var searchQuery = searchCommand.SearchText;
        if (string.IsNullOrEmpty(searchQuery))
        {
            var title = searchCommand.SearchType switch
            {
                SearchType.NameContains => "Search by Name",
                SearchType.NameRegex => "Search by Name (Regex)",
                _ => throw new ArgumentOutOfRangeException()
            };
            var containerNameInput = new TextInputElement(title);
            await _userCommunicationService.ReadInputs(containerNameInput);

            if (containerNameInput.Value is not null)
            {
                searchQuery = containerNameInput.Value;
            }
        }
        
        //TODO proper error message
        if(string.IsNullOrWhiteSpace(searchQuery)) return;
        
        var searchMatcher = searchCommand.SearchType switch
        {
            SearchType.NameContains => new NameContainsMatcher(_itemNameConverterService, searchQuery),
            //SearchType.NameRegex => new NameRegexMatcher(searchQuery),
            _ => throw new ArgumentOutOfRangeException()
        };

        await _searchManager.StartSearchAsync(searchMatcher, _currentLocation);
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