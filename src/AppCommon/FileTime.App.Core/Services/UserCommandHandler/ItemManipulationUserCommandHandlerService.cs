using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using DynamicData;
using FileTime.App.Core.Interactions;
using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Command;
using FileTime.Core.Command.CreateContainer;
using FileTime.Core.Command.CreateElement;
using FileTime.Core.Command.Move;
using FileTime.Core.Extensions;
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
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandScheduler _commandScheduler;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISystemClipboardService _systemClipboardService;
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
        IServiceProvider serviceProvider,
        ISystemClipboardService systemClipboardService) : base(appState, timelessContentProvider)
    {
        _userCommandHandlerService = userCommandHandlerService;
        _clipboardService = clipboardService;
        _userCommunicationService = userCommunicationService;
        _logger = logger;
        _timelessContentProvider = timelessContentProvider;
        _commandScheduler = commandScheduler;
        _serviceProvider = serviceProvider;
        _systemClipboardService = systemClipboardService;

        SaveSelectedTab(t => _selectedTab = t);
        SaveCurrentLocation(l => _currentLocation = l);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);

        _markedItems = appState.SelectedTab.Select(t => t?.MarkedItems).ToBindedCollection();

        AddCommandHandlers(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<CopyCommand>(Copy),
            new TypeUserCommandHandler<DeleteCommand>(Delete),
            new TypeUserCommandHandler<RenameCommand>(Rename),
            new TypeUserCommandHandler<MarkCommand>(MarkItem),
            new TypeUserCommandHandler<PasteCommand>(Paste),
            new TypeUserCommandHandler<CreateContainer>(CreateContainer),
            new TypeUserCommandHandler<CreateElement>(CreateElement),
            new TypeUserCommandHandler<PasteFilesFromClipboardCommand>(PasteFilesFromClipboard),
        });
    }

    private async Task PasteFilesFromClipboard(PasteFilesFromClipboardCommand arg)
    {
        await _systemClipboardService.GetFiles();
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
        _clipboardService.SetCommand<FileTime.Core.Command.Copy.CopyCommandFactory>();

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
            _clipboardService.AddContent(
                item.FullName
                ?? throw new ArgumentException($"{nameof(item.FullName)} can not be null.", nameof(item))
            );
        }

        return Task.CompletedTask;
    }

    private async Task Paste(PasteCommand command)
    {
        await (command.PasteMode switch
        {
            PasteMode.Merge => Paste(TransportMode.Merge),
            PasteMode.Overwrite => Paste(TransportMode.Overwrite),
            PasteMode.Skip => Paste(TransportMode.Skip),
            _ => throw new ArgumentException($"Unknown {nameof(PasteMode)} value: {command.PasteMode}")
        });
    }

    private async Task Paste(TransportMode mode)
    {
        if (_clipboardService.CommandFactoryType is null)
        {
            _userCommunicationService.ShowToastMessage("Clipboard is empty.");
            return;
        }

        //TODO: check _currentLocation?.FullName
        var commandFactory = (ITransportationCommandFactory) _serviceProvider.GetRequiredService(_clipboardService.CommandFactoryType);
        var command = commandFactory.GenerateCommand(_clipboardService.Content, mode, _currentLocation?.FullName);

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

    private async Task Rename(RenameCommand command)
    {
        if ((_markedItems?.Collection?.Count ?? 0) > 0)
        {
            BehaviorSubject<string> templateRegexValue = new(string.Empty);
            BehaviorSubject<string> newNameSchemaValue = new(string.Empty);

            var itemPreviews = _markedItems!.Collection!
                .Select(item =>
                    {
                        var originalName = item.GetName();

                        var decoratedOriginalName = templateRegexValue.Select(templateRegex =>
                            {
                                try
                                {
                                    if (string.IsNullOrWhiteSpace(templateRegex))
                                        return new List<ItemNamePart> {new(originalName)};

                                    var regex = new Regex(templateRegex);
                                    var match = regex.Match(originalName);
                                    if (!match.Success) return new List<ItemNamePart> {new(originalName)};

                                    var matchGroups = match.Groups;

                                    var indices = Enumerable.Empty<int>()
                                        .Prepend(0)
                                        .Concat(
                                            ((IList<Group>) match.Groups).Skip(1).SelectMany(g => new[] {g.Index, g.Index + g.Length})
                                        )
                                        .Append(originalName.Length)
                                        .ToList();

                                    var itemNameParts = new List<ItemNamePart>();
                                    for (var i = 0; i < indices.Count - 1; i++)
                                    {
                                        var text = originalName.Substring(indices[i], indices[i + 1] - indices[i]);
                                        itemNameParts.Add(new ItemNamePart(text, i % 2 == 1));
                                    }

                                    return itemNameParts;
                                }
                                catch
                                {
                                    return new List<ItemNamePart> {new(originalName)};
                                }
                            }
                        );

                        var text2 = Observable.CombineLatest(
                            templateRegexValue,
                            newNameSchemaValue,
                            (templateRegex, newNameSchema) =>
                            {
                                try
                                {
                                    if (string.IsNullOrWhiteSpace(templateRegex)
                                        || string.IsNullOrWhiteSpace(newNameSchema)) return new List<ItemNamePart> {new(originalName)};

                                    var regex = new Regex(templateRegex);
                                    var match = regex.Match(originalName);
                                    if (!match.Success) return new List<ItemNamePart> {new(originalName)};

                                    var matchGroups = match.Groups;

                                    var newNameParts = Enumerable.Range(1, matchGroups.Count).Aggregate(
                                        (IEnumerable<string>) new List<string> {newNameSchema},
                                        (acc, i) =>
                                            acc.SelectMany(item2 =>
                                                item2
                                                    .Split($"/{i}/")
                                                    .SelectMany(e => new[] {e, $"/{i}/"})
                                                    .SkipLast(1)
                                            )
                                    );


                                    var itemNameParts = newNameParts.Select(namePart =>
                                        namePart.StartsWith("/")
                                        && namePart.EndsWith("/")
                                        && namePart.Length > 2
                                        && int.TryParse(namePart.AsSpan(1, namePart.Length - 2), out var index)
                                        && index > 0
                                        && index <= matchGroups.Count
                                            ? new ItemNamePart(matchGroups[index].Value, true)
                                            : new ItemNamePart(namePart, false)
                                    );

                                    return itemNameParts.ToList();
                                }
                                catch
                                {
                                    return new List<ItemNamePart> {new(originalName)};
                                }
                            }
                        );

                        var preview = new DoubleTextPreview
                        {
                            Text1 = decoratedOriginalName,
                            Text2 = text2
                        };
                        return preview;
                    }
                );

            DoubleTextListPreview doubleTextListPreview = new();
            doubleTextListPreview.Items.AddRange(itemPreviews);

            var templateRegex = new TextInputElement("Template regex", string.Empty,
                s => templateRegexValue.OnNext(s!));
            var newNameSchema = new TextInputElement("New name schema", string.Empty,
                s => newNameSchemaValue.OnNext(s!));
            await _userCommunicationService.ReadInputs(
                new[] {templateRegex, newNameSchema},
                new[] {doubleTextListPreview}
            );
        }
        else
        {
            List<ItemToMove> itemsToMove = new();
            if (_currentSelectedItem?.BaseItem?.FullName is null) return;

            var item = await _timelessContentProvider.GetItemByFullNameAsync(_currentSelectedItem.BaseItem.FullName, PointInTime.Present);

            if (item is null) return;

            var renameInput = new TextInputElement("New name", item.Name);

            await _userCommunicationService.ReadInputs(renameInput);

            //TODO: should check these...
            var newPath = item.FullName!.GetParent()!.GetChild(renameInput.Value!);
            itemsToMove.Add(new ItemToMove(item.FullName, newPath));

            var moveCommandFactory = _serviceProvider.GetRequiredService<MoveCommandFactory>();
            var moveCommand = moveCommandFactory.GenerateCommand(itemsToMove);
            await AddCommand(moveCommand);
        }
    }

    private async Task Delete(DeleteCommand command)
    {
        IList<FullName>? itemsToDelete = null;
        var shouldDelete = false;
        string? questionText = null;
        if ((_markedItems?.Collection?.Count ?? 0) > 0)
        {
            itemsToDelete = new List<FullName>(_markedItems!.Collection!);
        }
        else if (_currentSelectedItem?.BaseItem?.FullName is not null)
        {
            itemsToDelete = new List<FullName>()
            {
                _currentSelectedItem.BaseItem.FullName
            };
        }

        if ((itemsToDelete?.Count ?? 0) == 0) return;

        if (itemsToDelete!.Count == 1)
        {
            var resolvedOnlyItem = await _timelessContentProvider.GetItemByFullNameAsync(itemsToDelete[0], PointInTime.Present);

            if (resolvedOnlyItem is IContainer {AllowRecursiveDeletion: true} onlyContainer
                && onlyContainer.ItemsCollection.Any())
            {
                questionText = $"The container '{onlyContainer.DisplayName}' is not empty. Proceed with delete?";
            }
            else
            {
                shouldDelete = true;
            }
        }

        if (itemsToDelete.Count == 0) return;

        if (questionText is { })
        {
            var proceedDelete = await _userCommunicationService.ShowMessageBox(questionText!);

            if (proceedDelete == MessageBoxResult.Cancel) return;
        }
        else if (!shouldDelete)
        {
            return;
        }


        var deleteCommand = _serviceProvider.GetRequiredService<FileTime.Core.Command.Delete.DeleteCommand>();
        deleteCommand.HardDelete = command.IsHardDelete;
        deleteCommand.ItemsToDelete.AddRange(itemsToDelete!);
        await AddCommand(deleteCommand);

        _selectedTab?.ClearMarkedItems();
    }

    private async Task AddCommand(ICommand command)
    {
        await _commandScheduler.AddCommand(command);
    }
}