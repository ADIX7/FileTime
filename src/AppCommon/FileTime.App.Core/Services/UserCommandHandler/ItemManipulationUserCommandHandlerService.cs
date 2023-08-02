using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using DeclarativeProperty;
using DynamicData;
using FileTime.App.Core.Interactions;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Command;
using FileTime.Core.Command.CreateContainer;
using FileTime.Core.Command.Move;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using InitableService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CreateElementCommand = FileTime.App.Core.UserCommand.CreateElementCommand;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class ItemManipulationUserCommandHandlerService : UserCommandHandlerServiceBase
{
    private ITabViewModel? _selectedTab;
    private IDeclarativeProperty<IItemViewModel?>? _currentSelectedItem;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly IClipboardService _clipboardService;
    private readonly IUserCommunicationService _userCommunicationService;
    private readonly ILogger<ItemManipulationUserCommandHandlerService> _logger;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ICommandScheduler _commandScheduler;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISystemClipboardService _systemClipboardService;
    private readonly IDeclarativeProperty<ObservableCollection<FullName>> _markedItems;
    private IDeclarativeProperty<IContainer?>? _currentLocation;

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

        _markedItems = appState.SelectedTab.Map(t => t?.MarkedItems).Switch();

        AddCommandHandlers(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<CopyCommand>(CopyAsync),
            new TypeUserCommandHandler<DeleteCommand>(DeleteAsync),
            new TypeUserCommandHandler<RenameCommand>(RenameAsync),
            new TypeUserCommandHandler<MarkCommand>(MarkItemAsync),
            new TypeUserCommandHandler<PasteCommand>(PasteAsync),
            new TypeUserCommandHandler<CreateContainer>(CreateContainerAsync),
            new TypeUserCommandHandler<CreateElementCommand>(CreateElementAsync),
            new TypeUserCommandHandler<PasteFilesFromClipboardCommand>(PasteFilesFromClipboardAsync),
            new TypeUserCommandHandler<CopyFilesToClipboardCommand>(CopyFilesToClipboardAsync),
        });
    }

    private async Task CopyFilesToClipboardAsync()
    {
        var list = new List<FullName>();
        if ((_markedItems.Value?.Count ?? 0) > 0)
        {
            list.AddRange(_markedItems.Value!);
        }
        else if(_currentSelectedItem?.Value?.BaseItem?.FullName is { } selectedItemName)
        {
            list.Add(selectedItemName);
        }

        if (list.Count > 0)
        {
            await _systemClipboardService.SetFilesAsync(list);
        }
    }

    private async Task PasteFilesFromClipboardAsync(PasteFilesFromClipboardCommand command) =>
        await (command.PasteMode switch
        {
            PasteMode.Merge => PasteFilesFromClipboardAsync(TransportMode.Merge),
            PasteMode.Overwrite => PasteFilesFromClipboardAsync(TransportMode.Overwrite),
            PasteMode.Skip => PasteFilesFromClipboardAsync(TransportMode.Skip),
            _ => throw new ArgumentException($"Unknown {nameof(PasteMode)} value: {command.PasteMode}")
        });

    private async Task PasteFilesFromClipboardAsync(TransportMode mode)
    {
        if (_currentLocation?.Value?.FullName is not { }) return;

        var files = (await _systemClipboardService.GetFilesAsync()).ToList();
        var copyCommandFactory = _serviceProvider.GetRequiredService<FileTime.Core.Command.Copy.CopyCommandFactory>();
        var copyCommand = copyCommandFactory.GenerateCommand(files, mode, _currentLocation.Value.FullName);

        await AddCommandAsync(copyCommand);
    }

    private async Task MarkItemAsync()
    {
        if (_selectedTab == null || _currentSelectedItem?.Value?.BaseItem?.FullName == null) return;

        _selectedTab.ToggleMarkedItem(_currentSelectedItem.Value.BaseItem.FullName);
        await _userCommandHandlerService.HandleCommandAsync(MoveCursorDownCommand.Instance);
    }

    private Task CopyAsync()
    {
        _clipboardService.Clear();
        _clipboardService.SetCommand<FileTime.Core.Command.Copy.CopyCommandFactory>();

        if ((_markedItems.Value?.Count ?? 0) > 0)
        {
            foreach (var item in _markedItems.Value!)
            {
                _clipboardService.AddContent(item);
            }

            _selectedTab?.ClearMarkedItems();
        }
        else if (_currentSelectedItem?.Value?.BaseItem != null)
        {
            var item = _currentSelectedItem.Value.BaseItem;
            _clipboardService.AddContent(
                item.FullName
                ?? throw new ArgumentException($"{nameof(item.FullName)} can not be null.", nameof(item))
            );
        }

        return Task.CompletedTask;
    }

    private async Task PasteAsync(PasteCommand command) =>
        await (command.PasteMode switch
        {
            PasteMode.Merge => PasteAsync(TransportMode.Merge),
            PasteMode.Overwrite => PasteAsync(TransportMode.Overwrite),
            PasteMode.Skip => PasteAsync(TransportMode.Skip),
            _ => throw new ArgumentException($"Unknown {nameof(PasteMode)} value: {command.PasteMode}")
        });

    private async Task PasteAsync(TransportMode mode)
    {
        if (_clipboardService.CommandFactoryType is null)
        {
            _userCommunicationService.ShowToastMessage("Clipboard is empty.");
            return;
        }

        //TODO: check _currentLocation?.FullName
        var commandFactory = (ITransportationCommandFactory) _serviceProvider.GetRequiredService(_clipboardService.CommandFactoryType);
        var command = commandFactory.GenerateCommand(_clipboardService.Content, mode, _currentLocation?.Value?.FullName);

        _clipboardService.Clear();

        if (command is IRequireInputCommand requireInput) await requireInput.ReadInputs();

        await AddCommandAsync(command);
    }

    private async Task CreateContainerAsync()
    {
        var containerNameInput = new TextInputElement("Container name");

        await _userCommunicationService.ReadInputs(containerNameInput);

        //TODO: message on empty result
        var newContainerName = containerNameInput.Value;

        if (_currentLocation?.Value?.FullName is null || newContainerName is null) return;

        var command = _serviceProvider
            .GetInitableResolver(_currentLocation.Value.FullName, newContainerName)
            .GetRequiredService<CreateContainerCommand>();
        await AddCommandAsync(command);
    }

    private async Task CreateElementAsync()
    {
        var containerNameInput = new TextInputElement("Element name");

        await _userCommunicationService.ReadInputs(containerNameInput);

        //TODO: message on empty result
        var newContainerName = containerNameInput.Value;

        if (_currentLocation?.Value?.FullName is null || newContainerName is null) return;

        var command = _serviceProvider
            .GetInitableResolver(_currentLocation.Value.FullName, newContainerName)
            .GetRequiredService<FileTime.Core.Command.CreateElement.CreateElementCommand>();
        await AddCommandAsync(command);
    }

    private async Task RenameAsync(RenameCommand command)
    {
        List<ItemToMove> itemsToMove = new();
        if ((_markedItems.Value?.Count ?? 0) > 0)
        {
            BehaviorSubject<string> templateRegexValue = new(string.Empty);
            BehaviorSubject<string> newNameSchemaValue = new(string.Empty);

            var itemsToRename = new List<FullName>(_markedItems.Value!);

            var itemPreviews = itemsToRename
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
                                    var itemNameParts = GetItemNameParts(regex, originalName, newNameSchema);

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

            var success = await _userCommunicationService.ReadInputs(
                new[] {templateRegex, newNameSchema},
                new[] {doubleTextListPreview}
            );

            if (success)
            {
                if (templateRegex.Value is null)
                {
                    //TODO messagebox
                }
                else if (newNameSchema.Value is null)
                {
                    //TODO messagebox
                }
                else
                {
                    var regex = new Regex(templateRegex.Value);
                    var itemsToMoveWithPath = itemsToRename
                        .Select(item =>
                            (
                                OriginalFullName: item,
                                NewName:
                                item.GetParent()!.GetChild(
                                    string.Join(
                                        "",
                                        GetItemNameParts(regex, item.GetName(), newNameSchema.Value)
                                            .Select(i => i.Text)
                                    )
                                )
                            )
                        )
                        .Select(i => new ItemToMove(i.OriginalFullName, i.NewName));

                    itemsToMove.AddRange(itemsToMoveWithPath);
                }
            }
        }
        else
        {
            if (_currentSelectedItem?.Value?.BaseItem?.FullName is null) return;

            var item = await _timelessContentProvider.GetItemByFullNameAsync(_currentSelectedItem.Value.BaseItem.FullName, PointInTime.Present);

            if (item is null) return;

            var renameInput = new TextInputElement("New name", item.Name);

            if (await _userCommunicationService.ReadInputs(renameInput))
            {
                //TODO: should check these null forgivings...
                var newPath = item.FullName!.GetParent()!.GetChild(renameInput.Value!);
                itemsToMove.Add(new ItemToMove(item.FullName, newPath));
            }
        }

        if (itemsToMove.Count > 0)
        {
            //TODO: name collision, probably on the input window at the new template name
            //TODO: check if the name changed
            var moveCommandFactory = _serviceProvider.GetRequiredService<MoveCommandFactory>();
            var moveCommand = moveCommandFactory.GenerateCommand(itemsToMove);
            await AddCommandAsync(moveCommand);
        }

        static IEnumerable<ItemNamePart> GetItemNameParts(Regex templateRegex, string originalName, string newNameSchema)
        {
            var match = templateRegex.Match(originalName);
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

            return itemNameParts;
        }
    }

    private async Task DeleteAsync(DeleteCommand command)
    {
        IList<FullName>? itemsToDelete = null;
        var shouldDelete = false;
        string? questionText = null;
        if ((_markedItems.Value?.Count ?? 0) > 0)
        {
            itemsToDelete = new List<FullName>(_markedItems.Value!);
        }
        else if (_currentSelectedItem?.Value?.BaseItem?.FullName is not null)
        {
            itemsToDelete = new List<FullName>()
            {
                _currentSelectedItem.Value.BaseItem.FullName
            };
        }

        if ((itemsToDelete?.Count ?? 0) == 0) return;

        if (itemsToDelete!.Count == 1)
        {
            var resolvedOnlyItem = await _timelessContentProvider.GetItemByFullNameAsync(itemsToDelete[0], PointInTime.Present);

            if (resolvedOnlyItem is IContainer {AllowRecursiveDeletion: true} onlyContainer
                && onlyContainer.Items.Count > 0)
            {
                questionText = $"The container '{onlyContainer.DisplayName}' is not empty. Proceed with delete?";
            }
            else
            {
                shouldDelete = true;
            }
        }
        else
        {
            shouldDelete = true;
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
        await AddCommandAsync(deleteCommand);

        _selectedTab?.ClearMarkedItems();
    }

    private async Task AddCommandAsync(ICommand command) 
        => await _commandScheduler.AddCommand(command);
}