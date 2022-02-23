using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileTime.App.Core.Clipboard;
using FileTime.App.Core.Command;
using FileTime.Avalonia.Application;
using FileTime.Avalonia.IconProviders;
using FileTime.Avalonia.Misc;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.ViewModels;
using FileTime.Core.Command;
using FileTime.Core.Command.Copy;
using FileTime.Core.Command.CreateContainer;
using FileTime.Core.Command.CreateElement;
using FileTime.Core.Command.Delete;
using FileTime.Core.Command.Move;
using FileTime.Core.Command.Rename;
using FileTime.Core.Components;
using FileTime.Core.ContainerSizeScanner;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Core.Search;
using FileTime.Core.Services;
using FileTime.Core.Timeline;
using FileTime.Providers.Local;
using FileTime.Tools.Compression.Command;
using Microsoft.Extensions.Logging;

namespace FileTime.Avalonia.Services
{
    public class CommandHandlerService
    {
        private bool _addCommandToNextBatch;

        private readonly AppState _appState;
        private readonly LocalContentProvider _localContentProvider;
        private readonly ItemNameConverterService _itemNameConverterService;
        private readonly IDialogService _dialogService;
        private readonly IClipboard _clipboard;
        private readonly TimeRunner _timeRunner;
        private readonly IIconProvider _iconProvider;
        private readonly IEnumerable<IContentProvider> _contentProviders;
        private readonly Dictionary<Commands, Func<Task>> _commandHandlers;
        private readonly ProgramsService _programsService;
        private readonly ILogger<CommandHandlerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ContainerScanSnapshotProvider _containerScanSnapshotProvider;

        public CommandHandlerService(
            AppState appState,
            LocalContentProvider localContentProvider,
            ItemNameConverterService itemNameConverterService,
            IDialogService dialogService,
            IClipboard clipboard,
            TimeRunner timeRunner,
            IIconProvider iconProvider,
            IEnumerable<IContentProvider> contentProviders,
            ProgramsService programsService,
            ILogger<CommandHandlerService> logger,
            IServiceProvider serviceProvider,
            ContainerScanSnapshotProvider containerScanSnapshotProvider)
        {
            _appState = appState;
            _localContentProvider = localContentProvider;
            _itemNameConverterService = itemNameConverterService;
            _dialogService = dialogService;
            _clipboard = clipboard;
            _timeRunner = timeRunner;
            _iconProvider = iconProvider;
            _contentProviders = contentProviders;
            _programsService = programsService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _containerScanSnapshotProvider = containerScanSnapshotProvider;

            _commandHandlers = new Dictionary<Commands, Func<Task>>
            {
                {Commands.AutoRefresh, ToggleAutoRefresh},
                {Commands.ChangeTimelineMode, ChangeTimelineMode},
                {Commands.CloseTab, CloseTab},
                {Commands.Compress, Compress},
                {Commands.Copy, Copy},
                {Commands.CopyHash, CopyHash},
                {Commands.CopyPath, CopyPath},
                {Commands.CreateContainer, CreateContainer},
                {Commands.CreateElement, CreateElement},
                {Commands.Cut, Cut},
                {Commands.Edit, Edit},
                {Commands.EnterRapidTravel, EnterRapidTravelMode},
                {Commands.FindByName, FindByName},
                {Commands.FindByNameRegex, FindByNameRegex},
                {Commands.GoToHome, GotToHome},
                {Commands.GoToPath, GoToContainer},
                {Commands.GoToProvider, GotToProvider},
                {Commands.GoToRoot, GotToRoot},
                {Commands.GoUp, GoUp},
                {Commands.HardDelete, HardDelete},
                {Commands.Mark, MarkCurrentItem},
                {Commands.MoveCursorDown, MoveCursorDown},
                {Commands.MoveCursorDownPage, MoveCursorDownPage},
                {Commands.MoveCursorUp, MoveCursorUp},
                {Commands.MoveCursorUpPage, MoveCursorUpPage},
                {Commands.MoveToFirst, MoveToFirst},
                {Commands.MoveToLast, MoveToLast},
                {Commands.NextTimelineBlock, SelectNextTimelineBlock},
                {Commands.NextTimelineCommand, SelectNextTimelineCommand},
                {Commands.Open, OpenContainer},
                {Commands.OpenInFileBrowser, OpenInDefaultFileExplorer},
                {Commands.OpenOrRun, OpenOrRun},
                {Commands.PasteMerge, PasteMerge},
                {Commands.PasteOverwrite, PasteOverwrite},
                {Commands.PasteSkip, PasteSkip},
                {Commands.PreviousTimelineBlock, SelectPreviousTimelineBlock},
                {Commands.PreviousTimelineCommand, SelectPreviousTimelineCommand},
                {Commands.Refresh, RefreshCurrentLocation},
                {Commands.Rename, Rename},
                {Commands.RunCommand, RunCommandInContainer},
                {Commands.ScanContainerSize, ScanContainerSize},
                {Commands.ShowAllShotcut, ShowAllShortcut},
                {Commands.SoftDelete, SoftDelete},
                {Commands.SwitchToLastTab, async() => await SwitchToTab(-1)},
                {Commands.SwitchToTab1, async() => await SwitchToTab(1)},
                {Commands.SwitchToTab2, async() => await SwitchToTab(2)},
                {Commands.SwitchToTab3, async() => await SwitchToTab(3)},
                {Commands.SwitchToTab4, async() => await SwitchToTab(4)},
                {Commands.SwitchToTab5, async() => await SwitchToTab(5)},
                {Commands.SwitchToTab6, async() => await SwitchToTab(6)},
                {Commands.SwitchToTab7, async() => await SwitchToTab(7)},
                {Commands.SwitchToTab8, async() => await SwitchToTab(8)},
                {Commands.TimelinePause, PauseTimeline},
                {Commands.TimelineRefresh, RefreshTimeline},
                {Commands.TimelineStart, ContinueTimeline},
                {Commands.ToggleAdvancedIcons, ToggleAdvancedIcons},
                {Commands.ToggleHidden, ToggleHidden},
            };
        }

        public async Task HandleCommandAsync(Commands command) =>
            await _commandHandlers[command].Invoke();

        private async Task OpenContainer()
        {
            _appState.RapidTravelText = "";
            await _appState.SelectedTab.Open();
        }

        public async Task OpenContainer(IContainer container)
        {
            _appState.RapidTravelText = "";
            await _appState.SelectedTab.OpenContainer(container);
        }

        private async Task OpenOrRun()
        {
            if (_appState.SelectedTab.SelectedItem is ContainerViewModel)
            {
                await OpenContainer();
            }
            else if (_appState.SelectedTab.SelectedItem is ElementViewModel elementViewModel && elementViewModel.Element is LocalFile localFile)
            {
                Process.Start(new ProcessStartInfo(localFile.File.FullName) { UseShellExecute = true });

                if (_appState.ViewMode == ViewMode.RapidTravel)
                {
                    await _appState.ExitRapidTravelMode();
                }
            }
        }

        private async Task GoUp()
        {
            await _appState.SelectedTab.GoUp();
        }

        private async Task MoveCursorUp()
        {
            await _appState.SelectedTab.MoveCursorUp();
        }

        private async Task MoveCursorDown()
        {
            await _appState.SelectedTab.MoveCursorDown();
        }

        private async Task MoveCursorUpPage()
        {
            await _appState.SelectedTab.MoveCursorUpPage();
        }

        private async Task MoveCursorDownPage()
        {
            await _appState.SelectedTab.MoveCursorDownPage();
        }

        private async Task MoveToFirst()
        {
            await _appState.SelectedTab.MoveCursorToFirst();
        }

        private async Task MoveToLast()
        {
            await _appState.SelectedTab.MoveCursorToLast();
        }

        private async Task GotToProvider()
        {
            await _appState.SelectedTab.GotToProvider();
        }

        private async Task GotToRoot()
        {
            await _appState.SelectedTab.GotToRoot();
        }

        private async Task GotToHome()
        {
            await _appState.SelectedTab.GotToHome();
        }

        private Task EnterRapidTravelMode()
        {
            _appState.ViewMode = ViewMode.RapidTravel;

            _appState.PreviousKeys.Clear();
            _appState.PossibleCommands = new();

            return Task.CompletedTask;
        }

        private async Task SwitchToTab(int number)
        {
            var tabContainer = _appState.Tabs.FirstOrDefault(t => t.TabNumber == number);

            if (number == -1)
            {
                var greatestNumber = _appState.Tabs.Max(t => t.TabNumber);
                tabContainer = _appState.Tabs.FirstOrDefault(t => t.TabNumber == greatestNumber);
            }
            else if (tabContainer == null)
            {
                var newContainer = await _appState.SelectedTab.CurrentLocation.Container.CloneAsync();

                var newTab = new Tab();
                await newTab.Init(newContainer);

                tabContainer = new TabContainer(_serviceProvider, _timeRunner, newTab, _localContentProvider, _itemNameConverterService, _appState);
                await tabContainer.Init(number);

                var i = 0;
                for (i = 0; i < _appState.Tabs.Count; i++)
                {
                    if (_appState.Tabs[i].TabNumber > number) break;
                }
                _appState.Tabs.Insert(i, tabContainer);
            }

            if (_appState.ViewMode == ViewMode.RapidTravel)
            {
                await _appState.ExitRapidTravelMode();
            }

            _appState.SelectedTab = tabContainer;
        }

        private async Task CloseTab()
        {
            var tabs = _appState.Tabs;
            if (tabs.Count > 1)
            {
                var currentTab = tabs.FirstOrDefault(t => t == _appState.SelectedTab);

                if (currentTab != null)
                {
                    tabs.Remove(currentTab);
                    var tabNumber = tabs[0].TabNumber;
                    for (var i = 0; i < tabs.Count; i++)
                    {
                        tabNumber = tabs[i].TabNumber;
                        if (tabs[i].TabNumber > currentTab.TabNumber) break;
                    }
                    await SwitchToTab(tabNumber);
                }
            }
        }

        private Task CreateContainer()
        {
            var handler = async (List<InputElementWrapper> inputs) =>
            {
                string? containerName = null;
                try
                {
                    containerName = inputs[0].Value;
                    var container = _appState.SelectedTab.CurrentLocation.Container;
                    var createContainerCommand = new CreateContainerCommand(new AbsolutePath(container), containerName);
                    await AddCommand(createContainerCommand);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while creating container {Container}", containerName);
                }
            };

            _dialogService.ReadInputs(new List<InputElement>() { InputElement.ForText("Container name") }, handler);

            return Task.CompletedTask;
        }

        private Task CreateElement()
        {
            var handler = async (List<InputElementWrapper> inputs) =>
            {
                string? elementName = null;
                try
                {
                    elementName = inputs[0].Value;
                    var container = _appState.SelectedTab.CurrentLocation.Container;
                    var createElementCommand = new CreateElementCommand(new AbsolutePath(container), elementName);
                    await AddCommand(createElementCommand);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while creating element {Element}", elementName);
                }
            };

            _dialogService.ReadInputs(new List<InputElement>() { InputElement.ForText("Element name") }, handler);

            return Task.CompletedTask;
        }

        private async Task MarkCurrentItem()
        {
            await _appState.SelectedTab.MarkCurrentItem();
        }

        private async Task Copy()
        {
            _clipboard.Clear();
            _clipboard.SetCommand<CopyCommand>();

            var currentSelectedItems = await _appState.SelectedTab.TabState.GetCurrentMarkedItems();
            if (currentSelectedItems.Count > 0)
            {
                foreach (var selectedItem in currentSelectedItems)
                {
                    _clipboard.AddContent(selectedItem);
                }
                await _appState.SelectedTab.TabState.ClearCurrentMarkedItems();
            }
            else
            {
                var currentSelectedItem = _appState.SelectedTab.SelectedItem?.Item;
                if (currentSelectedItem != null)
                {
                    _clipboard.AddContent(new AbsolutePath(currentSelectedItem));
                }
            }
        }

        private Task Cut()
        {
            _clipboard.Clear();
            _clipboard.SetCommand<MoveCommand>();

            return Task.CompletedTask;
        }

        private async Task SoftDelete() => await Delete(false);

        private async Task HardDelete() => await Delete(true);

        public async Task Delete(bool hardDelete = false)
        {
            IList<AbsolutePath>? itemsToDelete = null;
            var askForDelete = false;
            var questionText = "";
            var shouldDelete = false;
            var shouldClearMarkedItems = false;

            var currentSelectedItems = await _appState.SelectedTab.TabState.GetCurrentMarkedItems();
            var currentSelectedItem = _appState.SelectedTab.SelectedItem?.Item;
            if (currentSelectedItems.Count > 0)
            {
                itemsToDelete = new List<AbsolutePath>(currentSelectedItems);
                shouldClearMarkedItems = true;

                //FIXME: check 'is Container'
                if (currentSelectedItems.Count == 1)
                {
                    if ((await currentSelectedItems[0].ResolveAsync()) is IContainer container
                        && (await container.GetItems())?.Count > 0 && container.AllowRecursiveDeletion)
                    {
                        askForDelete = true;
                        questionText = $"The container '{container.Name}' is not empty. Proceed with delete?";
                    }
                    else
                    {
                        shouldDelete = true;
                    }
                }
                else
                {
                    askForDelete = true;
                    questionText = $"Are you sure you want to delete {itemsToDelete.Count} item?";
                }
            }
            else if (currentSelectedItem != null)
            {
                itemsToDelete = new List<AbsolutePath>()
                {
                    new AbsolutePath(currentSelectedItem)
                };

                if (currentSelectedItem is IContainer container && (await container.GetItems())?.Count > 0 && container.AllowRecursiveDeletion)
                {
                    askForDelete = true;
                    questionText = $"The container '{container.Name}' is not empty. Proceed with delete?";
                }
                else
                {
                    shouldDelete = true;
                }
            }

            if (itemsToDelete?.Count > 0)
            {
                if (askForDelete)
                {
                    _dialogService.ShowMessageBox(questionText, HandleDelete);
                }
                else if (shouldDelete)
                {
                    await HandleDelete();
                }
            }

            async Task HandleDelete()
            {
                var deleteCommand = new DeleteCommand
                {
                    HardDelete = hardDelete
                };

                foreach (var itemToDelete in itemsToDelete!)
                {
                    deleteCommand.ItemsToDelete.Add(itemToDelete);
                }

                await AddCommand(deleteCommand);
                _clipboard.Clear();
                if (shouldClearMarkedItems)
                {
                    await _appState.SelectedTab.TabState.ClearCurrentMarkedItems();
                }
            }
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

        private async Task Paste(TransportMode transportMode)
        {
            if (_clipboard.CommandType != null)
            {
                var command = (ITransportationCommand)Activator.CreateInstance(_clipboard.CommandType!)!;
                command.TransportMode = transportMode;

                command.Sources.Clear();

                foreach (var item in _clipboard.Content)
                {
                    command.Sources.Add(item);
                }

                _clipboard.Clear();

                var currentLocation = _appState.SelectedTab.CurrentLocation.Container;

                currentLocation =
                    currentLocation is VirtualContainer virtualContainer
                    ? virtualContainer.BaseContainer
                    : currentLocation;

                if (!command.TargetIsContainer)
                {
                    var handler = async (List<InputElementWrapper> inputs) =>
                    {
                        command.Target = AbsolutePath.FromParentAndChildName(currentLocation, inputs[0].Value, AbsolutePathType.Element);
                        command.InputResults = inputs.Skip(1).Select(i => i.Option ?? i.Value).ToList();
                        await AddCommand(command);
                    };

                    var inputs = new List<InputElement>()
                    {
                        InputElement.ForText("Compressed element name")
                    };
                    inputs.AddRange(command.Inputs);

                    _dialogService.ReadInputs(inputs, handler);
                }
                else
                {
                    command.Target = new AbsolutePath(currentLocation);
                    await AddCommand(command);
                }
            }
        }

        private Task Rename()
        {
            var selectedItem = _appState.SelectedTab.SelectedItem?.Item;
            if (selectedItem != null)
            {
                var handler = async (List<InputElementWrapper> inputs) =>
                {
                    var renameCommand = new RenameCommand(new AbsolutePath(selectedItem), inputs[0].Value);
                    await AddCommand(renameCommand);
                };

                _dialogService.ReadInputs(new List<InputElement>() { InputElement.ForText("New name", selectedItem.Name) }, handler);
            }
            return Task.CompletedTask;
        }

        private async Task RefreshCurrentLocation()
        {
            await _appState.SelectedTab.CurrentLocation.Container.RefreshAsync();
            await _appState.SelectedTab.UpdateCurrentSelectedItem();
        }

        private Task PauseTimeline()
        {
            _timeRunner.EnableRunning = false;
            return Task.CompletedTask;
        }

        private async Task ContinueTimeline()
        {
            _timeRunner.EnableRunning = true;
            await _timeRunner.TryStartCommandRunner();
        }

        private async Task RefreshTimeline()
        {
            await _timeRunner.Refresh();
        }

        private Task ChangeTimelineMode()
        {
            _addCommandToNextBatch = !_addCommandToNextBatch;
            _dialogService.ShowToastMessage("Timeline mode: " + (_addCommandToNextBatch ? "Continuous" : "Parallel"));

            return Task.CompletedTask;
        }

        private Task GoToContainer()
        {
            var handler = async (List<InputElementWrapper> inputs) =>
            {
                var path = inputs[0].Value;
                foreach (var contentProvider in _contentProviders)
                {
                    if (await contentProvider.CanHandlePath(path))
                    {
                        var possibleContainer = await contentProvider.GetByPath(path);
                        if (possibleContainer is IContainer container)
                        {
                            await _appState.SelectedTab.OpenContainer(container);
                        }
                        //TODO: multiple possible content provider handler
                        return;
                    }
                }
            };

            _dialogService.ReadInputs(new List<InputElement>() { InputElement.ForText("Path") }, handler);

            return Task.CompletedTask;
        }

        private Task ToggleAdvancedIcons()
        {
            _iconProvider.EnableAdvancedIcons = !_iconProvider.EnableAdvancedIcons;
            _dialogService.ShowToastMessage("Advanced icons are: " + (_iconProvider.EnableAdvancedIcons ? "ON" : "OFF"));

            return Task.CompletedTask;
        }

        private Task ToggleHidden()
        {
            throw new NotImplementedException();
        }

        private Task OpenInDefaultFileExplorer()
        {
            if (_appState.SelectedTab.CurrentLocation.Container is LocalFolder localFolder)
            {
                var path = localFolder.NativePath;
                if (path != null)
                {
                    Process.Start("explorer.exe", "\"" + path + "\"");
                }
            }

            return Task.CompletedTask;
        }

        private async Task CopyPath()
        {
            var currentContainer = _appState.SelectedTab.CurrentLocation.Container;
            var textToCopy = currentContainer.NativePath;

            if (textToCopy != null)
            {
                await CopyToClipboard(textToCopy);
            }
        }

        private static async Task CopyToClipboard(string text)
        {
            if (global::Avalonia.Application.Current?.Clipboard is global::Avalonia.Input.Platform.IClipboard clipboard)
            {
                await clipboard.SetTextAsync(text);
            }
        }

        private Task ShowAllShortcut()
        {
            _appState.IsAllShortcutVisible = true;
            return Task.CompletedTask;
        }

        private Task RunCommandInContainer()
        {
            var handler = (List<InputElementWrapper> inputs) =>
            {
                var input = inputs[0].Value;
                string? path = null;
                string? arguments = null;

                if (input.StartsWith("\""))
                {
                    var pathEnd = input.IndexOf('\"', 1);

                    path = input.Substring(1, pathEnd);
                    arguments = input.Substring(pathEnd + 1).Trim();
                }
                else
                {
                    var inputParts = input.Split(' ');
                    path = inputParts[0];
                    arguments = inputParts.Length > 1 ? string.Join(' ', inputParts[1..]).Trim() : null;
                }

                if (!string.IsNullOrWhiteSpace(path))
                {
                    using var process = new Process();
                    process.StartInfo.FileName = path;

                    if (!string.IsNullOrWhiteSpace(arguments))
                    {
                        process.StartInfo.Arguments = arguments;
                    }
                    if (_appState.SelectedTab.CurrentLocation.Container is LocalFolder localFolder)
                    {
                        process.StartInfo.WorkingDirectory = localFolder.Directory.FullName;
                    }
                    process.Start();
                }

                return Task.CompletedTask;
            };

            _dialogService.ReadInputs(new List<InputElement>() { InputElement.ForText("Command") }, handler);

            return Task.CompletedTask;
        }

        private Task SelectPreviousTimelineBlock()
        {
            var currentSelected = GetSelectedTimelineCommandOrSelectFirst();
            if (currentSelected == null) return Task.CompletedTask;

            ParallelCommandsViewModel? newBlockVM = null;
            ParallelCommandsViewModel? previousBlockVM = null;

            foreach (var timelineBlock in _appState.TimelineCommands)
            {
                foreach (var command in timelineBlock.ParallelCommands)
                {
                    if (command.IsSelected)
                    {
                        newBlockVM = previousBlockVM;
                        break;
                    }
                }

                previousBlockVM = timelineBlock;
            }

            if (newBlockVM == null) return Task.CompletedTask;

            foreach (var val in _appState.TimelineCommands.Select(t => t.ParallelCommands.Select((c, i) => (ParalellCommandVM: t, CommandVM: c, Index: i))).SelectMany(t => t))
            {
                val.CommandVM.IsSelected = val.ParalellCommandVM == newBlockVM && val.Index == 0;
            }

            return Task.CompletedTask;
        }

        private Task SelectNextTimelineCommand()
        {
            var currentSelected = GetSelectedTimelineCommandOrSelectFirst();
            if (currentSelected == null) return Task.CompletedTask;

            ParallelCommandViewModel? lastCommand = null;
            var any = false;
            foreach (var command in _appState.TimelineCommands.SelectMany(t => t.ParallelCommands))
            {
                var isSelected = lastCommand == currentSelected;
                command.IsSelected = isSelected;
                any = any || isSelected;
                lastCommand = command;
            }
            if (!any && lastCommand != null) lastCommand.IsSelected = true;
            return Task.CompletedTask;
        }

        private Task SelectPreviousTimelineCommand()
        {
            var currentSelected = GetSelectedTimelineCommandOrSelectFirst();
            if (currentSelected == null) return Task.CompletedTask;

            ParallelCommandViewModel? lastCommand = null;
            foreach (var command in _appState.TimelineCommands.SelectMany(t => t.ParallelCommands))
            {
                if (lastCommand != null)
                {
                    lastCommand.IsSelected = command == currentSelected;
                }
                lastCommand = command;
            }
            if (lastCommand != null) lastCommand.IsSelected = false;
            return Task.CompletedTask;
        }

        private Task SelectNextTimelineBlock()
        {
            var currentSelected = GetSelectedTimelineCommandOrSelectFirst();
            if (currentSelected == null) return Task.CompletedTask;

            ParallelCommandsViewModel? newBlockVM = null;
            var select = false;
            foreach (var timelineBlock in _appState.TimelineCommands)
            {
                if (select)
                {
                    newBlockVM = timelineBlock;
                    break;
                }
                foreach (var command in timelineBlock.ParallelCommands)
                {
                    if (command.IsSelected)
                    {
                        select = true;
                        break;
                    }
                }
            }

            if (newBlockVM == null) return Task.CompletedTask;

            foreach (var val in _appState.TimelineCommands.Select(t => t.ParallelCommands.Select((c, i) => (ParalellCommandVM: t, CommandVM: c, Index: i))).SelectMany(t => t))
            {
                val.CommandVM.IsSelected = val.ParalellCommandVM == newBlockVM && val.Index == 0;
            }

            return Task.CompletedTask;
        }

        private ParallelCommandViewModel? GetSelectedTimelineCommandOrSelectFirst()
        {
            var currentSelected = _appState.TimelineCommands.SelectMany(t => t.ParallelCommands).FirstOrDefault(c => c.IsSelected);
            if (currentSelected != null) return currentSelected;

            var firstCommand = _appState.TimelineCommands.SelectMany(t => t.ParallelCommands).FirstOrDefault();
            if (firstCommand != null)
            {
                firstCommand.IsSelected = true;
            }

            return null;
        }

        private async Task AddCommand(ICommand command)
        {
            if (_addCommandToNextBatch)
            {
                await _timeRunner.AddCommand(command, toNewBatch: true);
            }
            else
            {
                ParallelCommandsViewModel? batchToAdd = null;
                foreach (var val in _appState.TimelineCommands.Select(t => t.ParallelCommands.Select(c => (ParalellCommandVM: t, CommandVM: c))).SelectMany(t => t))
                {
                    if (val.CommandVM.IsSelected)
                    {
                        batchToAdd = val.ParalellCommandVM;
                        break;
                    }
                }

                if (batchToAdd != null)
                {
                    await _timeRunner.AddCommand(command, batchToAdd.Id);
                }
                else
                {
                    await _timeRunner.AddCommand(command);
                }
            }
        }

        private Task ToggleAutoRefresh()
        {
            var tab = _appState.SelectedTab.TabState.Tab;
            tab.AutoRefresh = !tab.AutoRefresh;

            _dialogService.ShowToastMessage("Auto refresh is: " + (tab.AutoRefresh ? "ON" : "OFF"));

            return Task.CompletedTask;
        }

        private Task Edit()
        {
            if (_appState.SelectedTab.SelectedItem?.Item is IElement element && element.NativePath is string filePath)
            {
                var getNext = false;
                while (true)
                {
                    string? execPath = null;
                    try
                    {
                        var editorProgram = _programsService.GetEditorProgram(getNext);
                        if (editorProgram is null)
                        {
                            break;
                        }
                        else if (editorProgram.Path is string executablePath)
                        {
                            execPath = executablePath;
                            if (string.IsNullOrWhiteSpace(editorProgram.Arguments))
                            {
                                Process.Start(executablePath, "\"" + filePath + "\"");
                            }
                            else
                            {
                                var parts = editorProgram.Arguments.Split("%%1");
                                var arguments = string.Join("%%1", parts.Select(p => p.Replace("%1", "\"" + filePath + "\""))).Replace("%%1", "%1");
                                Process.Start(executablePath, arguments);
                            }
                        }
                        //TODO: else
                        break;
                    }
                    catch (System.ComponentModel.Win32Exception e)
                    {
                        _logger.LogError(e, "Error while running editor program, possible the executable path does not exists. {ExecutablePath}", execPath);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Unkown error while running editor program.");
                    }
                    getNext = true;
                }
            }
            //TODO: else
            return Task.CompletedTask;
        }

        private Task CopyHash()
        {
            var handler = async (List<InputElementWrapper> inputs) =>
            {
                var hashFunction = (HashFunction?)inputs[0].Option;
                if (hashFunction != null && _appState.SelectedTab.SelectedItem?.Item is IElement element && element.Provider.SupportsContentStreams)
                {
                    using var stream = new ContentProviderStream(await element.GetContentReaderAsync());

                    string? hashString = null;
                    using HashAlgorithm hashFunc = hashFunction switch
                    {
                        HashFunction.MD5 => MD5.Create(),
                        HashFunction.SHA256 => SHA256.Create(),
                        HashFunction.SHA384 => SHA384.Create(),
                        HashFunction.SHA512 => SHA512.Create(),
                        _ => throw new NotImplementedException()
                    };

                    var hash = hashFunc.ComputeHash(stream);
                    hashString = string.Concat(hash.Select(b => b.ToString("X2")));

                    _dialogService.ShowToastMessage($"Hash copied ({hashString})");
                    await CopyToClipboard(hashString);
                }
            };

            _dialogService.ReadInputs(new List<InputElement>()
            {
                InputElement.ForOptions("Hash function", Enum.GetValues<HashFunction>().Cast<object>())
            }, handler);

            return Task.CompletedTask;
        }

        private async Task Compress()
        {
            _clipboard.Clear();
            _clipboard.SetCommand<CompressCommand>();

            var currentSelectedItems = await _appState.SelectedTab.TabState.GetCurrentMarkedItems();
            if (currentSelectedItems.Count > 0)
            {
                foreach (var selectedItem in currentSelectedItems)
                {
                    _clipboard.AddContent(selectedItem);
                }
                await _appState.SelectedTab.TabState.ClearCurrentMarkedItems();
            }
            else
            {
                var currentSelectedItem = _appState.SelectedTab.SelectedItem?.Item;
                if (currentSelectedItem != null)
                {
                    _clipboard.AddContent(new AbsolutePath(currentSelectedItem));
                }
            }
        }

        private Task FindByName()
        {
            var handler = async (List<InputElementWrapper> inputs) =>
            {
                var container = _appState.SelectedTab.CurrentLocation.Container;
                while (container is SearchContainer searchContainer)
                {
                    container = searchContainer.SearchBaseContainer;
                }

                var search = new NameSearchTask(inputs[0].Value, container, _itemNameConverterService);
                search.Start();

                await OpenContainer(search.TargetContainer);
            };

            _dialogService.ReadInputs(new List<InputElement>()
            {
                InputElement.ForText("Item name pattern")
            }, handler);

            return Task.CompletedTask;
        }

        private Task FindByNameRegex()
        {
            var handler = async (List<InputElementWrapper> inputs) =>
            {
                var container = _appState.SelectedTab.CurrentLocation.Container;
                while (container is SearchContainer searchContainer)
                {
                    container = searchContainer.SearchBaseContainer;
                }

                var search = new NameRegexSearchTask(inputs[0].Value, container);
                search.Start();

                await OpenContainer(search.TargetContainer);
            };

            _dialogService.ReadInputs(new List<InputElement>()
            {
                InputElement.ForText("Item name pattern")
            }, handler);

            return Task.CompletedTask;
        }

        private async Task ScanContainerSize()
        {
            if (_appState.SelectedTab.CurrentLocation != null)
            {
                var scanTask = new ScanSizeTask(_containerScanSnapshotProvider, _appState.SelectedTab.CurrentLocation.Container);
                scanTask.Start();

                await OpenContainer(scanTask.Snapshot);
            }
        }
    }
}