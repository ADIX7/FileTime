using FileTime.Core.Components;
using FileTime.Core.Interactions;
using FileTime.Providers.Local;
using FileTime.Uno.Application;
using FileTime.Uno.Command;
using FileTime.Uno.Misc;
using FileTime.Uno.Models;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.System;

namespace FileTime.Uno.ViewModels
{
    [ViewModel]
    [Inject(typeof(LocalContentProvider))]
    [Inject(typeof(AppState), PropertyAccessModifier = AccessModifier.Public)]
    public partial class MainPageViewModel
    {
        private readonly List<KeyWithModifiers> _previousKeys = new List<KeyWithModifiers>();
        private bool _isAltPressed = false;
        private bool _isShiftPressed = false;
        private bool _isCtrlPressed = false;

        private Action _inputHandler;

        [Property]
        private string _text;

        [Property]
        private List<CommandBinding> _commandBindings = new();

        [Property]
        private bool _noCommandFound;

        [Property]
        private List<CommandBinding> _possibleCommands = new();

        [Property]
        private List<InputElementWrapper> _inputs;

        [Property]
        private List<RootDriveInfo> _rootDriveInfos;

        public Action FocusDefaultElement { get; set; }

        partial void OnInitialize()
        {
            InitCommandBindings();

            AppState.Tabs = new List<TabContainer>()
            {
                new TabContainer(new Tab(LocalContentProvider), LocalContentProvider)
            };

            var driveInfos = new List<RootDriveInfo>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                var container = LocalContentProvider.RootContainers.FirstOrDefault(d => d.Name == drive.Name.TrimEnd(Path.DirectorySeparatorChar));
                if (container != null)
                {
                    var driveInfo = new RootDriveInfo(drive, container);
                    driveInfos.Add(driveInfo);
                }
            }

            RootDriveInfos = driveInfos;
        }

        public void OpenContainer()
        {
            AppState.SelectedTab.Open();
        }

        public void GoUp()
        {
            AppState.SelectedTab.GoUp();
        }

        public void MoveCursorUp()
        {
            AppState.SelectedTab.MoveCursorUp();
        }

        public void MoveCursorDown()
        {
            AppState.SelectedTab.MoveCursorDown();
        }

        public void MoveCursorUpPage()
        {
            AppState.SelectedTab.MoveCursorUpPage();
        }

        public void MoveCursorDownPage()
        {
            AppState.SelectedTab.MoveCursorDownPage();
        }

        public void MoveToFirst()
        {
            AppState.SelectedTab.MoveCursorToFirst();
        }

        public void MoveToLast()
        {
            AppState.SelectedTab.MoveCursorToLast();
        }

        public void GotToProvider()
        {
            AppState.SelectedTab.GotToProvider();
        }

        public void GotToRoot()
        {
            AppState.SelectedTab.GotToRoot();
        }

        public void GotToHome()
        {
            AppState.SelectedTab.GotToHome();
        }

        public void CreateContainer()
        {
            var handler = () =>
            {
                if (Inputs != null)
                {
                    AppState.SelectedTab.CreateContainer(Inputs[0].Value);
                    Inputs = null;
                }
            };

            ReadInputs(new List<InputElement>() { new InputElement("Container name", InputType.Text) }, handler);
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

        public bool ProcessKeyDown(VirtualKey key)
        {
            NoCommandFound = false;
            if (key == VirtualKey.Menu)
            {
                _isAltPressed = true;
            }
            else if (key == VirtualKey.Shift)
            {
                _isShiftPressed = true;
            }
            else if (key == VirtualKey.Control)
            {
                _isCtrlPressed = true;
            }
            return false;
        }

        public bool ProcessKeyUp(VirtualKey key)
        {
            if (key == VirtualKey.Menu)
            {
                _isAltPressed = false;
            }
            else if (key == VirtualKey.Shift)
            {
                _isShiftPressed = false;
            }
            else if (key == VirtualKey.Control)
            {
                _isCtrlPressed = false;
            }
            else
            {
                var keyWithModifiers = new KeyWithModifiers(key, _isAltPressed, _isShiftPressed, _isCtrlPressed);
                _previousKeys.Add(keyWithModifiers);

                var selectedCommandBinding = _commandBindings.Find(c => AreKeysEqual(c.Keys, _previousKeys));

                if (key == VirtualKey.Escape)
                {
                    _previousKeys.Clear();
                    PossibleCommands = new();
                }
                else if (_previousKeys.Count == 2 && selectedCommandBinding == null)
                {
                    NoCommandFound = true;
                    _previousKeys.Clear();
                    PossibleCommands = new();
                }
                else if (selectedCommandBinding != null)
                {
                    selectedCommandBinding.Invoke();
                    _previousKeys.Clear();
                    PossibleCommands = new();

                    if (selectedCommandBinding.Name != "")
                    {
                        FocusDefaultElement?.Invoke();
                    }
                }
                else
                {
                    var possibleCommands = _commandBindings.Where(c => AreKeysEqual(c.Keys[0], keyWithModifiers)).ToList();

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
                return true;
            }
            return false;
        }

        public void ResetSpecialKeys()
        {
            _isAltPressed = false;
            _isShiftPressed = false;
            _isCtrlPressed = false;
        }

        private void ReadInputs(List<InputElement> inputs, Action inputHandler)
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
                new CommandBinding("go up", FileTime.App.Core.Command.Commands.GoUp, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.Left)}, GoUp),
                new CommandBinding("open", FileTime.App.Core.Command.Commands.Open, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.Right)}, OpenContainer),
                /*new CommandBinding("cursor up", FileTime.App.Core.Command.Commands.MoveCursorUp, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.Up)}, MoveCursorUp),
                new CommandBinding("cursor down", FileTime.App.Core.Command.Commands.MoveCursorDown, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.Down)}, MoveCursorDown),
                new CommandBinding("cursor page up", FileTime.App.Core.Command.Commands.MoveCursorUpPage, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.PageUp)}, MoveCursorUpPage),
                new CommandBinding("cursor page down", FileTime.App.Core.Command.Commands.MoveCursorDownPage, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.PageDown)}, MoveCursorDownPage),*/
                new CommandBinding("", null, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.Up)}, () =>{ }),
                new CommandBinding("", null, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.Down)}, () =>{ }),
                new CommandBinding("", null, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.PageUp)}, () =>{ }),
                new CommandBinding("", null, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.PageDown)}, () =>{ }),
                new CommandBinding("", null, new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.Tab)}, () =>{ }),
                new CommandBinding(
                    "create container",
                    FileTime.App.Core.Command.Commands.CreateContainer,
                    new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.C),new KeyWithModifiers(VirtualKey.C)},
                    CreateContainer),
                new CommandBinding(
                    "move to first",
                    FileTime.App.Core.Command.Commands.MoveToTop,
                    new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.G),new KeyWithModifiers(VirtualKey.G)},
                    MoveToFirst),
                new CommandBinding(
                    "move to last",
                    FileTime.App.Core.Command.Commands.MoveToBottom,
                    new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.G, shift: true)},
                    MoveToLast),
                new CommandBinding(
                    "go to provider",
                    FileTime.App.Core.Command.Commands.GoToProvider,
                    new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.G),new KeyWithModifiers(VirtualKey.T)},
                    GotToProvider),
                new CommandBinding(
                    "go to root",
                    FileTime.App.Core.Command.Commands.GoToRoot,
                    new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.G),new KeyWithModifiers(VirtualKey.R)},
                    GotToRoot),
                new CommandBinding(
                    "go to home",
                    FileTime.App.Core.Command.Commands.GoToHome,
                    new KeyWithModifiers[]{new KeyWithModifiers(VirtualKey.G),new KeyWithModifiers(VirtualKey.H)},
                    GotToHome),
            };

            _commandBindings.AddRange(commandBindings);
        }
    }
}
