using System.Reactive.Linq;
using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Command;
using FileTime.Core.Command.CreateContainer;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
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
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandScheduler _commandScheduler;
    private readonly BindedCollection<FullName>? _markedItems;
    private PointInTime _currentPointInTime;
    private IContainer? _currentLocation;

    public ItemManipulationUserCommandHandlerService(
        IAppState appState,
        IUserCommandHandlerService userCommandHandlerService,
        IClipboardService clipboardService,
        IInputInterface inputInterface,
        ILogger<ItemManipulationUserCommandHandlerService> logger,
        ITimelessContentProvider timelessContentProvider,
        ICommandScheduler commandScheduler) : base(appState, timelessContentProvider)
    {
        _userCommandHandlerService = userCommandHandlerService;
        _clipboardService = clipboardService;
        _inputInterface = inputInterface;
        _logger = logger;
        _timelessContentProvider = timelessContentProvider;
        _commandScheduler = commandScheduler;
        _currentPointInTime = null!;

        SaveSelectedTab(t => _selectedTab = t);
        SaveCurrentLocation(l => _currentLocation = l);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);
        SaveCurrentPointInTime(t => _currentPointInTime = t);

        _markedItems = new BindedCollection<FullName>(appState.SelectedTab.Select(t => t?.MarkedItems));

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

        _selectedTab.ToggleMarkedItem(_currentSelectedItem.BaseItem.FullName);
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
            var item = _currentSelectedItem.BaseItem;
            _clipboardService.AddContent(item.FullName ?? throw new ArgumentException($"{nameof(item.FullName)} can not be null.", nameof(item)));
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

        if (_currentLocation?.FullName is null || newContainerName is null) return;

        var command = new CreateContainerCommand(_currentLocation.FullName, newContainerName, _timelessContentProvider);
        await _commandScheduler.AddCommand(command);
    }
}