using System.Reactive.Linq;
using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Command;
using FileTime.Core.Command.CreateContainer;
using FileTime.Core.Command.CreateElement;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using InitableService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class ItemManipulationUserCommandHandlerService : UserCommandHandlerServiceBase
{
    private ITabViewModel? _selectedTab;
    private IItemViewModel? _currentSelectedItem;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly IClipboardService _clipboardService;
    private readonly IUserCommunicationService _userCommunicationService;
    private readonly ILogger<ItemManipulationUserCommandHandlerService> _logger;
    private readonly ICommandScheduler _commandScheduler;
    private readonly IServiceProvider _serviceProvider;
    private readonly BindedCollection<FullName>? _markedItems;
    private IContainer? _currentLocation;

    public ItemManipulationUserCommandHandlerService(
        IAppState appState,
        IUserCommandHandlerService userCommandHandlerService,
        IClipboardService clipboardService,
        IUserCommunicationService userCommunicationService,
        ILogger<ItemManipulationUserCommandHandlerService> logger,
        ITimelessContentProvider timelessContentProvider,
        ICommandScheduler commandScheduler,
        IServiceProvider serviceProvider) : base(appState, timelessContentProvider)
    {
        _userCommandHandlerService = userCommandHandlerService;
        _clipboardService = clipboardService;
        _userCommunicationService = userCommunicationService;
        _logger = logger;
        _commandScheduler = commandScheduler;
        _serviceProvider = serviceProvider;

        SaveSelectedTab(t => _selectedTab = t);
        SaveCurrentLocation(l => _currentLocation = l);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);

        _markedItems = new BindedCollection<FullName>(appState.SelectedTab.Select(t => t?.MarkedItems));

        AddCommandHandlers(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<CopyCommand>(Copy),
            new TypeUserCommandHandler<MarkCommand>(MarkItem),
            new TypeUserCommandHandler<PasteCommand>(Paste),
            new TypeUserCommandHandler<CreateContainer>(CreateContainer),
            new TypeUserCommandHandler<CreateElement>(CreateElement),
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
        _clipboardService.SetCommand<FileTime.Core.Command.Copy.CopyCommand>();

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
            _clipboardService.AddContent(item.FullName ??
                                         throw new ArgumentException($"{nameof(item.FullName)} can not be null.",
                                             nameof(item)));
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

    private async Task Paste(TransportMode mode)
    {
        if (_clipboardService.CommandType is null)
        {
            _userCommunicationService.ShowToastMessage("Clipboard is empty.");
            return;
        }

        var command = (ITransportationCommand) _serviceProvider.GetRequiredService(_clipboardService.CommandType);
        command.TransportMode = mode;

        command.Sources.Clear();

        foreach (var item in _clipboardService.Content)
        {
            command.Sources.Add(item);
        }

        command.Target = _currentLocation?.FullName;

        _clipboardService.Clear();

        if (command is IRequireInputCommand requireInput) await requireInput.ReadInputs();
        
        await AddCommand(command);
    }

    private async Task CreateContainer()
    {
        var containerNameInput = new TextInputElement("Container name");

        await _userCommunicationService.ReadInputs(containerNameInput);

        //TODO: message on empty result
        var newContainerName = containerNameInput.Value;

        if (_currentLocation?.FullName is null || newContainerName is null) return;

        var command = _serviceProvider
            .GetInitableResolver(_currentLocation.FullName, newContainerName)
            .GetRequiredService<CreateContainerCommand>();
        await AddCommand(command);
    }

    private async Task CreateElement()
    {
        var containerNameInput = new TextInputElement("Element name");

        await _userCommunicationService.ReadInputs(containerNameInput);

        //TODO: message on empty result
        var newContainerName = containerNameInput.Value;

        if (_currentLocation?.FullName is null || newContainerName is null) return;

        var command = _serviceProvider
            .GetInitableResolver(_currentLocation.FullName, newContainerName)
            .GetRequiredService<CreateElementCommand>();
        await AddCommand(command);
    }

    private async Task AddCommand(ICommand command)
    {
        await _commandScheduler.AddCommand(command);
    }
}