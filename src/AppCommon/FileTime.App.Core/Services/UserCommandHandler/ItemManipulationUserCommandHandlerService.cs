using System.Reactive.Linq;
using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Command;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using Microsoft.Extensions.Logging;
using CopyCommand = FileTime.Core.Command.Copy.CopyCommand;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class ItemManipulationUserCommandHandlerService : UserCommandHandlerServiceBase
{
    private ITabViewModel? _selectedTab;
    private IItemViewModel? _currentSelectedItem;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly IClipboardService _clipboardService;
    private readonly IInputInterface _inputInterface;
    private readonly ILogger<ItemManipulationUserCommandHandlerService> _logger;
    private readonly BindedCollection<IAbsolutePath>? _markedItems;

    public ItemManipulationUserCommandHandlerService(
        IAppState appState,
        IUserCommandHandlerService userCommandHandlerService,
        IClipboardService clipboardService,
        IInputInterface inputInterface,
        ILogger<ItemManipulationUserCommandHandlerService> logger) : base(appState)
    {
        _userCommandHandlerService = userCommandHandlerService;
        _clipboardService = clipboardService;
        _inputInterface = inputInterface;
        _logger = logger;

        SaveSelectedTab(t => _selectedTab = t);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);

        _markedItems = new BindedCollection<IAbsolutePath>(appState.SelectedTab.Select(t => t?.MarkedItems));

        AddCommandHandlers(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<CopyCommand>(Copy),
            new TypeUserCommandHandler<MarkCommand>(MarkItem),
            new TypeUserCommandHandler<PasteCommand>(Paste),
            new TypeUserCommandHandler<CreateContainer>(CreateContainer),
        });
    }

    private async Task MarkItem()
    {
        if (_selectedTab == null || _currentSelectedItem?.BaseItem?.FullName == null) return;

        _selectedTab.ToggleMarkedItem(new AbsolutePath(_currentSelectedItem.BaseItem));
        await _userCommandHandlerService.HandleCommandAsync(MoveCursorDownCommand.Instance);
    }

    private Task Copy()
    {
        _clipboardService.Clear();
        _clipboardService.SetCommand<CopyCommand>();

        if ((_markedItems?.Collection?.Count ?? 0) > 0)
        {
            foreach (var item in _markedItems!.Collection!)
            {
                _clipboardService.AddContent(item);
            }

            _selectedTab?.ClearMarkedItems();
        }
        else if (_currentSelectedItem?.BaseItem != null)
        {
            _clipboardService.AddContent(new AbsolutePath(_currentSelectedItem.BaseItem));
        }

        return Task.CompletedTask;
    }

    private async Task Paste(PasteCommand command)
    {
        await (command.PasteMode switch
        {
            PasteMode.Merge => PasteMerge(),
            PasteMode.Overwrite => PasteOverwrite(),
            PasteMode.Skip => PasteSkip(),
            _ => throw new ArgumentException($"Unknown {nameof(PasteMode)} value: {command.PasteMode}")
        });
    }

    private async Task PasteMerge()
    {
        await Paste(TransportMode.Merge);
    }

    private async Task PasteOverwrite()
    {
        await Paste(TransportMode.Overwrite);
    }

    private async Task PasteSkip()
    {
        await Paste(TransportMode.Skip);
    }

    private Task Paste(TransportMode skip)
    {
        if (_clipboardService.CommandType is null) return Task.CompletedTask;
        return Task.CompletedTask;
    }

    private async Task CreateContainer()
    {
        var containerNameInput = new TextInputElement("Container name");

        await _inputInterface.ReadInputs(new List<IInputElement>() { containerNameInput });
        
        //TODO: message on empty result
        var newContainerName = containerNameInput.Value;
    }
}