using FileTime.Core.Components;
using FileTime.Core.Extensions;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Providers.Local;
using FileTime.Avalonia.Application;
using FileTime.Avalonia.Command;
using FileTime.Avalonia.Misc;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.Services;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Input;

namespace FileTime.Avalonia.ViewModels
{
    [ViewModel]
    [Inject(typeof(LocalContentProvider))]
    [Inject(typeof(AppState), PropertyAccessModifier = AccessModifier.Public)]
    [Inject(typeof(ItemNameConverterService))]
    public partial class MainPageViewModel
    {
        const string RAPIDTRAVEL = "rapidTravel";

        private readonly List<KeyWithModifiers> _previousKeys = new List<KeyWithModifiers>();
        private readonly List<KeyWithModifiers[]> _keysToSkip = new List<KeyWithModifiers[]>();
        private List<CommandBinding> _commandBindings = new();
        private List<CommandBinding> _universalCommandBindings = new();

        private Action? _inputHandler;

        [Property]
        private string _text;

        [Property]
        private bool _noCommandFound;

        [Property]
        private List<CommandBinding> _possibleCommands = new();

        [Property]
        private List<InputElementWrapper> _inputs;

        [Property]
        private List<RootDriveInfo> _rootDriveInfos;

        public Action? FocusDefaultElement { get; set; }

        async partial void OnInitialize()
        {
            InitCommandBindings();

            _keysToSkip.Add(new KeyWithModifiers[] { new KeyWithModifiers(Key.Up) });
            _keysToSkip.Add(new KeyWithModifiers[] { new KeyWithModifiers(Key.Down) });
            _keysToSkip.Add(new KeyWithModifiers[] { new KeyWithModifiers(Key.Tab) });
            _keysToSkip.Add(new KeyWithModifiers[] { new KeyWithModifiers(Key.PageDown) });
            _keysToSkip.Add(new KeyWithModifiers[] { new KeyWithModifiers(Key.PageUp) });

            var tab = new Tab();
            await tab.Init(LocalContentProvider);

            var tabContainer = new TabContainer(tab, LocalContentProvider, ItemNameConverterService);
            await tabContainer.Init(1);
            tabContainer.IsSelected = true;
            AppState.Tabs.Add(tabContainer);

            var driveInfos = new List<RootDriveInfo>();
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
            {
                var container = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? await GetContainerForWindowsDrive(drive)
                    : await GetContainerForLinuxDrive(drive);
                if (container != null)
                {
                    var driveInfo = new RootDriveInfo(drive, container);
                    driveInfos.Add(driveInfo);
                }
            }

            RootDriveInfos = driveInfos.OrderBy(d => d.Name).ToList();
        }

        private async Task<IContainer> GetContainerForWindowsDrive(DriveInfo drive)
        {
            return (await LocalContentProvider.GetRootContainers()).FirstOrDefault(d => d.Name == drive.Name.TrimEnd(Path.DirectorySeparatorChar));
        }

        private async Task<IContainer> GetContainerForLinuxDrive(DriveInfo drive)
        {
            return await LocalContentProvider.GetByPath(drive.Name) as IContainer;
        }

        public async Task OpenContainer()
        {
            AppState.RapidTravelText = "";
            await AppState.SelectedTab.Open();
        }

        public async Task OpenOrRun()
        {
            if (AppState.SelectedTab.SelectedItem is ContainerViewModel)
            {
                await OpenContainer();
            }
            else if (AppState.SelectedTab.SelectedItem is ElementViewModel elementViewModel && elementViewModel.Element is LocalFile localFile)
            {
                Process.Start(new ProcessStartInfo(localFile.File.FullName) { UseShellExecute = true });

                if (AppState.ViewMode == ViewMode.RapidTravel)
                {
                    await ExitRapidTravelMode();
                }
            }
        }

        public async Task GoUp()
        {
            await AppState.SelectedTab.GoUp();
        }

        public async Task MoveCursorUp()
        {
            await AppState.SelectedTab.MoveCursorUp();
        }

        public async Task MoveCursorDown()
        {
            await AppState.SelectedTab.MoveCursorDown();
        }

        public async Task MoveCursorUpPage()
        {
            await AppState.SelectedTab.MoveCursorUpPage();
        }

        public async Task MoveCursorDownPage()
        {
            await AppState.SelectedTab.MoveCursorDownPage();
        }

        public async Task MoveToFirst()
        {
            await AppState.SelectedTab.MoveCursorToFirst();
        }

        public async Task MoveToLast()
        {
            await AppState.SelectedTab.MoveCursorToLast();
        }

        public async Task GotToProvider()
        {
            await AppState.SelectedTab.GotToProvider();
        }

        public async Task GotToRoot()
        {
            await AppState.SelectedTab.GotToRoot();
        }

        public async Task GotToHome()
        {
            await AppState.SelectedTab.GotToHome();
        }

        public Task EnterRapidTravelMode()
        {
            AppState.ViewMode = ViewMode.RapidTravel;

            _previousKeys.Clear();
            PossibleCommands = new();
            FocusDefaultElement?.Invoke();

            return Task.CompletedTask;
        }

        public async Task ExitRapidTravelMode()
        {
            AppState.ViewMode = ViewMode.Default;

            _previousKeys.Clear();
            PossibleCommands = new();
            AppState.RapidTravelText = "";

            await AppState.SelectedTab.OpenContainer(await AppState.SelectedTab.CurrentLocation.Container.WithoutVirtualContainer(RAPIDTRAVEL));
            FocusDefaultElement?.Invoke();
        }

        public async Task SwitchToTab(int number)
        {
            var tabContainer = AppState.Tabs.FirstOrDefault(t => t.TabNumber == number);

            if (number == -1)
            {
                var greatestNumber = AppState.Tabs.Select(t => t.TabNumber).Max();
                tabContainer = AppState.Tabs.FirstOrDefault(t => t.TabNumber == greatestNumber);
            }
            else if (tabContainer == null)
            {
                var newContainer = await AppState.SelectedTab.CurrentLocation.Container.Clone();

                var newTab = new Tab();
                await newTab.Init(newContainer);

                tabContainer = new TabContainer(newTab, LocalContentProvider, ItemNameConverterService);
                await tabContainer.Init(number);

                var i = 0;
                for (i = 0; i < AppState.Tabs.Count; i++)
                {
                    if (AppState.Tabs[i].TabNumber > number) break;
                }
                AppState.Tabs.Insert(i, tabContainer);
            }

            if (AppState.ViewMode == ViewMode.RapidTravel)
            {
                await ExitRapidTravelMode();
            }

            AppState.SelectedTab = tabContainer;

            foreach (var tab2 in AppState.Tabs)
            {
                tab2.IsSelected = tab2.TabNumber == tabContainer!.TabNumber;
            }
        }

        public async Task CloseTab()
        {
            var tabs = AppState.Tabs;
            if (tabs.Count > 1)
            {
                var currentTab = tabs.FirstOrDefault(t => t == AppState.SelectedTab);

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

        public Task CreateContainer()
        {
            var handler = () =>
            {
                if (Inputs != null)
                {
                    AppState.SelectedTab.CreateContainer(Inputs[0].Value).Wait();
                    Inputs = null;
                }
            };

            ReadInputs(new List<Core.Interactions.InputElement>() { new Core.Interactions.InputElement("Container name", InputType.Text) }, handler);

            return Task.CompletedTask;
        }

        [Command]
        public void ProcessInputs()
        {
            _inputHandler();

            Inputs = null;
            _inputHandler = null;
        }

        [Command]
        public void CancelInputs()
        {
            Inputs = null;
            _inputHandler = null;
        }

        public async Task<bool> ProcessKeyDown(Key key, KeyModifiers keyModifiers)
        {
            NoCommandFound = false;

            var isAltPressed = (keyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt;
            var isShiftPressed = (keyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;
            var isCtrlPressed = (keyModifiers & KeyModifiers.Control) == KeyModifiers.Control;

            if (AppState.ViewMode == ViewMode.Default)
            {
                var keyWithModifiers = new KeyWithModifiers(key, isAltPressed, isShiftPressed, isCtrlPressed);
                _previousKeys.Add(keyWithModifiers);

                var selectedCommandBinding = _universalCommandBindings.Find(c => AreKeysEqual(c.Keys, _previousKeys));
                selectedCommandBinding ??= _commandBindings.Find(c => AreKeysEqual(c.Keys, _previousKeys));

                if (key == Key.Escape)
                {
                    _previousKeys.Clear();
                    PossibleCommands = new();
                }
                else if (selectedCommandBinding != null)
                {
                    await selectedCommandBinding.InvokeAsync();
                    _previousKeys.Clear();
                    PossibleCommands = new();

                    FocusDefaultElement?.Invoke();
                }
                else if (_keysToSkip.Any(k => AreKeysEqual(k, _previousKeys)))
                {
                    _previousKeys.Clear();
                    PossibleCommands = new();
                }
                else if (_previousKeys.Count == 2)
                {
                    NoCommandFound = true;
                    _previousKeys.Clear();
                    PossibleCommands = new();
                }
                else
                {
                    var possibleCommands = _universalCommandBindings.Concat(_commandBindings).Where(c => AreKeysEqual(c.Keys[0], keyWithModifiers)).ToList();

                    if (possibleCommands.Count == 0)
                    {
                        NoCommandFound = true;
                        _previousKeys.Clear();
                    }
                    else
                    {
                        PossibleCommands = possibleCommands;
                    }
                }
            }
            else
            {
                var keyString = key.ToString();
                var updateRapidTravelFilter = false;

                if (key == Key.Escape)
                {
                    await ExitRapidTravelMode();
                }
                else if (key == Key.Back)
                {
                    if (AppState.RapidTravelText.Length > 0)
                    {
                        AppState.RapidTravelText = AppState.RapidTravelText.Substring(0, AppState.RapidTravelText.Length - 1);
                        updateRapidTravelFilter = true;
                    }
                }
                else if (keyString.Length == 1)
                {
                    AppState.RapidTravelText += keyString.ToString().ToLower();
                    updateRapidTravelFilter = true;
                }
                else
                {
                    var currentKeyAsList = new List<KeyWithModifiers>() { new KeyWithModifiers(key) };
                    var selectedCommandBinding = _universalCommandBindings.Find(c => AreKeysEqual(c.Keys, currentKeyAsList));
                    if (selectedCommandBinding != null)
                    {
                        await selectedCommandBinding.InvokeAsync();
                        FocusDefaultElement?.Invoke();
                    }
                }

                if (updateRapidTravelFilter)
                {
                    var currentLocation = await AppState.SelectedTab.CurrentLocation.Container.WithoutVirtualContainer(RAPIDTRAVEL);
                    var newLocation = new VirtualContainer(
                        currentLocation,
                        new List<Func<IEnumerable<IContainer>, IEnumerable<IContainer>>>()
                        {
                                container => container.Where(c => c.Name.ToLower().Contains(AppState.RapidTravelText))
                        },
                        new List<Func<IEnumerable<IElement>, IEnumerable<IElement>>>()
                        {
                                element => element.Where(e => e.Name.ToLower().Contains(AppState.RapidTravelText))
                        },
                        virtualContainerName: RAPIDTRAVEL
                    );

                    await newLocation.Init();

                    await AppState.SelectedTab.OpenContainer(newLocation);

                    var selectedItemName = AppState.SelectedTab.SelectedItem?.Item.Name;
                    if (!(await AppState.SelectedTab.CurrentLocation.GetItems()).Select(i => i.Item.Name).Any(n => n == selectedItemName))
                    {
                        await AppState.SelectedTab.MoveCursorToFirst();
                    }

                    FocusDefaultElement?.Invoke();
                }

            }

            return true;
        }

        public Task<bool> ProcessKeyUp(Key key, KeyModifiers keyModifiers)
        {
            return Task.FromResult(false);
        }

        private void ReadInputs(List<Core.Interactions.InputElement> inputs, Action inputHandler)
        {
            Inputs = inputs.Select(i => new InputElementWrapper(i)).ToList();
            _inputHandler = inputHandler;
        }

        private static bool AreKeysEqual(IReadOnlyList<KeyWithModifiers> collection1, IReadOnlyList<KeyWithModifiers> collection2)
        {
            if (collection1.Count != collection2.Count) return false;

            for (var i = 0; i < collection1.Count; i++)
            {
                if (!AreKeysEqual(collection1[i], collection2[i])) return false;
            }

            return true;
        }

        private static bool AreKeysEqual(KeyWithModifiers key1, KeyWithModifiers key2) =>
            key1.Key == key2.Key
            && key1.Alt == key2.Alt
            && key1.Shift == key2.Shift
            && key1.Ctrl == key1.Ctrl;

        private void InitCommandBindings()
        {
            var commandBindings = new List<CommandBinding>()
            {
                new CommandBinding(
                    "enter rapid travel mode",
                    FileTime.App.Core.Command.Commands.EnterRapidTravel,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.OemComma, shift: true)},
                    EnterRapidTravelMode),
                new CommandBinding(
                    "create container",
                    FileTime.App.Core.Command.Commands.CreateContainer,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.C),new KeyWithModifiers(Key.C)},
                    CreateContainer),
                new CommandBinding(
                    "move to first",
                    FileTime.App.Core.Command.Commands.MoveToTop,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.G),new KeyWithModifiers(Key.G)},
                    MoveToFirst),
                new CommandBinding(
                    "move to last",
                    FileTime.App.Core.Command.Commands.MoveToBottom,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.G, shift: true)},
                    MoveToLast),
                new CommandBinding(
                    "go to provider",
                    FileTime.App.Core.Command.Commands.GoToProvider,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.G),new KeyWithModifiers(Key.T)},
                    GotToProvider),
                new CommandBinding(
                    "go to root",
                    FileTime.App.Core.Command.Commands.GoToRoot,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.G),new KeyWithModifiers(Key.R)},
                    GotToRoot),
                new CommandBinding(
                    "go to home",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.G),new KeyWithModifiers(Key.H)},
                    GotToHome),
                new CommandBinding(
                    "switch to tab 1",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.D1)},
                    async() => await SwitchToTab(1)),
                new CommandBinding(
                    "switch to tab 2",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.D2)},
                    async() => await SwitchToTab(2)),
                new CommandBinding(
                    "switch to tab 3",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.D3)},
                    async() => await SwitchToTab(3)),
                new CommandBinding(
                    "switch to tab 4",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.D4)},
                    async() => await SwitchToTab(4)),
                new CommandBinding(
                    "switch to tab 5",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.D5)},
                    async() => await SwitchToTab(5)),
                new CommandBinding(
                    "switch to tab 6",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.D6)},
                    async() => await SwitchToTab(6)),
                new CommandBinding(
                    "switch to tab 7",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.D7)},
                    async() => await SwitchToTab(7)),
                new CommandBinding(
                    "switch to tab 8",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.D8)},
                    async() => await SwitchToTab(8)),
                new CommandBinding(
                    "switch to last tab",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.D9)},
                    async() => await SwitchToTab(-1)),
                new CommandBinding(
                    "close tab",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(Key.Q)},
                    CloseTab),
            };
            var universalCommandBindings = new List<CommandBinding>()
            {
                new CommandBinding("go up", FileTime.App.Core.Command.Commands.GoUp, new KeyWithModifiers[]{new KeyWithModifiers(Key.Left)}, GoUp),
                new CommandBinding("open", FileTime.App.Core.Command.Commands.Open, new KeyWithModifiers[]{new KeyWithModifiers(Key.Right)}, OpenContainer),
                new CommandBinding("open or run", FileTime.App.Core.Command.Commands.OpenOrRun, new KeyWithModifiers[]{new KeyWithModifiers(Key.Enter)}, OpenOrRun),
                new CommandBinding("cursor up", FileTime.App.Core.Command.Commands.MoveCursorUp, new KeyWithModifiers[]{new KeyWithModifiers(Key.Up)}, MoveCursorUp),
                new CommandBinding("cursor down", FileTime.App.Core.Command.Commands.MoveCursorDown, new KeyWithModifiers[]{new KeyWithModifiers(Key.Down)}, MoveCursorDown),
                new CommandBinding("cursor page up", FileTime.App.Core.Command.Commands.MoveCursorUpPage, new KeyWithModifiers[]{new KeyWithModifiers(Key.PageUp)}, MoveCursorUpPage),
                new CommandBinding("cursor page down", FileTime.App.Core.Command.Commands.MoveCursorDownPage, new KeyWithModifiers[]{new KeyWithModifiers(Key.PageDown)}, MoveCursorDownPage),
            };

            _commandBindings.AddRange(commandBindings);
            _universalCommandBindings.AddRange(universalCommandBindings);
        }
    }
}
