using System.Diagnostics;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.App.Search;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class ToolUserCommandHandlerService : UserCommandHandlerServiceBase
{
    private readonly ISystemClipboardService _systemClipboardService;
    private readonly IUserCommunicationService _userCommunicationService;
    private readonly ISearchManager _searchManager;
    private readonly IItemNameConverterService _itemNameConverterService;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private IContainer? _currentLocation;
    private IItemViewModel? _currentSelectedItem;

    public ToolUserCommandHandlerService(
        IAppState appState,
        ISystemClipboardService systemClipboardService,
        IUserCommunicationService userCommunicationService,
        ISearchManager searchManager,
        IItemNameConverterService itemNameConverterService,
        ITimelessContentProvider timelessContentProvider,
        IUserCommandHandlerService userCommandHandlerService,
        IContentAccessorFactory contentAccessorFactory) : base(appState)
    {
        _systemClipboardService = systemClipboardService;
        _userCommunicationService = userCommunicationService;
        _searchManager = searchManager;
        _itemNameConverterService = itemNameConverterService;
        _timelessContentProvider = timelessContentProvider;
        _userCommandHandlerService = userCommandHandlerService;
        _contentAccessorFactory = contentAccessorFactory;
        SaveCurrentLocation(l => _currentLocation = l);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);

        AddCommandHandlers(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<OpenInDefaultFileExplorerCommand>(OpenInDefaultFileExplorer),
            new TypeUserCommandHandler<CopyNativePathCommand>(CopyNativePath),
            new TypeUserCommandHandler<CopyBase64Command>(CopyBase64),
            new TypeUserCommandHandler<SearchCommand>(Search),
        });
    }

    private async Task CopyBase64()
    {
        var item = _currentSelectedItem?.BaseItem;
        if (item?.Type != AbsolutePathType.Element || item is not IElement element) return;

        var contentReader = await _contentAccessorFactory.GetContentReaderFactory(element.Provider).CreateContentReaderAsync(element);
        using var ms = new MemoryStream();
        while (true)
        {
            //TODO handle large files
            var data = await contentReader.ReadBytesAsync(1048576);
            if (data.Length == 0) break;
            await ms.WriteAsync(data);
        }

        var base64Hash = Convert.ToBase64String(ms.ToArray());
        await _systemClipboardService.CopyToClipboardAsync(base64Hash);
    }

    private async Task Search(SearchCommand searchCommand)
    {
        if (_currentLocation is null) return;

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
        if (string.IsNullOrWhiteSpace(searchQuery)) return;

        var searchMatcher = searchCommand.SearchType switch
        {
            SearchType.NameContains => new NameContainsMatcher(_itemNameConverterService, searchQuery),
            //SearchType.NameRegex => new NameRegexMatcher(searchQuery),
            _ => throw new ArgumentOutOfRangeException()
        };

        var searchTask = await _searchManager.StartSearchAsync(searchMatcher, _currentLocation);
        var openContainerCommand = new OpenContainerCommand(new AbsolutePath(_timelessContentProvider, searchTask.SearchContainer));
        await _userCommandHandlerService.HandleCommandAsync(openContainerCommand);
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