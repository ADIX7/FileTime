using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.App.Core.Services;
using FileTime.App.Core.Services.UserCommandHandler;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;

namespace FileTime.Tools.Compression;

public class CompressionUserCommandHandler : AggregatedUserCommandHandler
{
    private readonly IClipboardService _clipboardService;
    private readonly IDeclarativeProperty<ObservableCollection<FullName>?> _markedItems;
    private readonly IDeclarativeProperty<IItemViewModel?> _selectedItem;

    public CompressionUserCommandHandler(
        IAppState appState,
        IClipboardService clipboardService)
    {
        _clipboardService = clipboardService;
        _markedItems = appState.SelectedTab.Map(t => t?.MarkedItems).Switch();
        _selectedItem = appState.SelectedTab.Map(t => t?.CurrentSelectedItem).Switch();

        AddCommandHandler(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<CompressUserCommand>(Compress),
            new TypeUserCommandHandler<DecompressUserCommand>(Decompress)
        });
    }

    private Task Decompress()
    {
        _clipboardService.Clear();
        _clipboardService.SetCommand<DecompressCommandFactory>();

        if (_markedItems.Value is {Count: > 0} markedItems)
        {
            foreach (var markedItem in markedItems)
            {
                _clipboardService.AddContent(markedItem);
            }
        }
        else if (_selectedItem.Value?.BaseItem?.FullName is { } fullname)
        {
            //TODO: check if file is decompressable
            _clipboardService.AddContent(fullname);
        }

        return Task.CompletedTask;
    }

    private Task Compress()
    {
        _clipboardService.Clear();
        _clipboardService.SetCommand<CompressCommandFactory>();

        if (_markedItems.Value is {Count: > 0} markedItems)
        {
            foreach (var markedItem in markedItems)
            {
                _clipboardService.AddContent(markedItem);
            }
        }
        else if (_selectedItem.Value?.BaseItem?.FullName is { } fullname)
        {
            _clipboardService.AddContent(fullname);
        }

        return Task.CompletedTask;
    }
}