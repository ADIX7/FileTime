using FileTime.Core.Models;
using FileTime.ConsoleUI.App.UI;
using FileTime.ConsoleUI.App.Command;
using FileTime.Core.Components;
using FileTime.Core.Extensions;
using FileTime.App.Core.Clipboard;

using Microsoft.Extensions.DependencyInjection;
using FileTime.App.Core.Tab;
using FileTime.ConsoleUI.App.UI.Color;
using FileTime.Core.Command;
using FileTime.App.Core.Command;
using FileTime.Core.Timeline;

namespace FileTime.ConsoleUI.App
{
    public partial class Application
    {
        private readonly List<Tab> _tabs = new();
        private readonly Dictionary<Tab, Render> _renderers = new();
        private readonly Dictionary<Tab, TabState> _tabStates = new();
        private Tab? _selectedTab;

        private readonly List<CommandBinding> _commandBindings = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly IClipboard _clipboard;
        private readonly IColoredConsoleRenderer _coloredConsoleRenderer;
        private readonly TimeRunner _timeRunner;
        private readonly ConsoleReader _consoleReader;
        private readonly IStyles _styles;
        private readonly List<ConsoleKeyInfo> _previousKeys = new();

        public bool IsRunning { get; private set; } = true;

        public Application(
            IServiceProvider serviceProvider,
            IClipboard clipboard,
            IColoredConsoleRenderer coloredConsoleRenderer,
            TimeRunner timeRunner,
            ConsoleReader consoleReader,
            IStyles styles)
        {
            _serviceProvider = serviceProvider;
            _clipboard = clipboard;
            _coloredConsoleRenderer = coloredConsoleRenderer;
            _timeRunner = timeRunner;
            _consoleReader = consoleReader;
            _styles = styles;
            InitCommandBindings();
        }

        public async Task SetContainer(IContainer currentPath)
        {
            _selectedTab = await CreateTab(currentPath);
        }

        private async Task<Tab> CreateTab(IContainer container)
        {
            var tab = new Tab();
            await tab.Init(container);
            _tabs.Add(tab);

            var paneState = new TabState(tab);
            _tabStates.Add(tab, paneState);

            var renderer = _serviceProvider.GetService<Render>()!;
            renderer.Init(tab, paneState);
            _renderers.Add(tab, renderer);

            return tab;
        }

        private void RemoveTab(Tab pane)
        {
            _tabs.Remove(pane);
            _renderers.Remove(pane);
            _tabStates.Remove(pane);
        }

        private void InitCommandBindings()
        {
            var commandBindings = new List<CommandBinding>()
            {
                new CommandBinding("close pane", Commands.CloseTab, new[] { new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false) }, CloseTab),
                new CommandBinding("cursor up", Commands.MoveCursorUp, new[] { new ConsoleKeyInfo('↑', ConsoleKey.UpArrow, false, false, false) }, MoveCursorUp),
                new CommandBinding("cursor down", Commands.MoveCursorDown, new[] { new ConsoleKeyInfo('↓', ConsoleKey.DownArrow, false, false, false) }, MoveCursorDown),
                new CommandBinding("cursor page up", Commands.MoveCursorUpPage, new[] { new ConsoleKeyInfo(' ', ConsoleKey.PageUp, false, false, false) }, MoveCursorUpPage),
                new CommandBinding("cursor page down", Commands.MoveCursorDownPage, new[] { new ConsoleKeyInfo(' ', ConsoleKey.PageDown, false, false, false) }, MoveCursorDownPage),
                new CommandBinding("go up", Commands.GoUp, new[] { new ConsoleKeyInfo('←', ConsoleKey.LeftArrow, false, false, false) }, GoUp),
                new CommandBinding("open", Commands.Open, new[] { new ConsoleKeyInfo('→', ConsoleKey.RightArrow, false, false, false) }, Open),
                new CommandBinding(
                    "go to top",
                    Commands.MoveToTop,
                    new[]
                    {
                        new ConsoleKeyInfo('g', ConsoleKey.G, false, false, false),
                        new ConsoleKeyInfo('g', ConsoleKey.G, false, false, false)
                    },
                    MoveCursorToTop),
                new CommandBinding(
                    "go to bottom",
                    Commands.MoveToBottom,
                    new[]
                    {
                        new ConsoleKeyInfo('G', ConsoleKey.G, true, false, false)
                    },
                    MoveCursorToBottom),
                new CommandBinding(
                    "toggle hidden",
                    Commands.ToggleHidden,
                    new[]
                    {
                        new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false),
                        new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false)
                    },
                    ToggleHidden),
                new CommandBinding("select", Commands.Select, new[] { new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false) }, Select),
                new CommandBinding(
                    "copy",
                    Commands.Copy,
                    new[]
                    {
                        new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false),
                        new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false)
                    },
                    Copy),
                new CommandBinding(
                    "cut",
                    Commands.Cut,
                    new[]
                    {
                        new ConsoleKeyInfo('d', ConsoleKey.D, false, false, false),
                        new ConsoleKeyInfo('d', ConsoleKey.D, false, false, false)
                    },
                    Cut),
                new CommandBinding(
                    "paste (merge)",
                    Commands.PasteMerge,
                    new[]
                    {
                        new ConsoleKeyInfo('p', ConsoleKey.P, false, false, false),
                        new ConsoleKeyInfo('p', ConsoleKey.P, false, false, false)
                    },
                    PasteMerge),
                new CommandBinding(
                    "paste (overwrite)",
                    Commands.PasteOverwrite,
                    new[]
                    {
                        new ConsoleKeyInfo('p', ConsoleKey.P, false, false, false),
                        new ConsoleKeyInfo('o', ConsoleKey.O, false, false, false)
                    },
                    PasteOverwrite),
                new CommandBinding(
                    "paste (skip)",
                    Commands.PasteSkip,
                    new[]
                    {
                        new ConsoleKeyInfo('p', ConsoleKey.P, false, false, false),
                        new ConsoleKeyInfo('s', ConsoleKey.S, false, false, false)
                    },
                    PasteSkip),
                new CommandBinding(
                    "create container",
                    Commands.CreateContainer,
                    new[]
                    {
                        new ConsoleKeyInfo('c', ConsoleKey.C, false, false, false),
                        new ConsoleKeyInfo('c', ConsoleKey.C, false, false, false)
                    },
                    CreateContainer),
                new CommandBinding(
                    "delete",
                    Commands.CreateContainer,
                    new[]
                    {
                        new ConsoleKeyInfo('d', ConsoleKey.D, false, false, false),
                        new ConsoleKeyInfo('D', ConsoleKey.D, true, false, false)
                    },
                    HardDelete),
            };

            _commandBindings.AddRange(commandBindings);
        }

        public async Task PrintUI(CancellationToken token = default)
        {
            if (_selectedTab != null)
            {
                await _renderers[_selectedTab].PrintUI(token);
            }
        }

        public async Task<bool> ProcessKey(ConsoleKeyInfo keyinfo)
        {
            var key = keyinfo.Key;
            _previousKeys.Add(keyinfo);

            var selectedCommandBinding = _commandBindings.Find(c => AreKeysEqual(c.Keys, _previousKeys));

            if (keyinfo.Key == ConsoleKey.Escape)
            {
                _previousKeys.Clear();
                return true;
            }
            else if (_previousKeys.Count == 2 && selectedCommandBinding == null)
            {
                HandleNoCommandFound();
                return false;
            }
            else if (selectedCommandBinding != null)
            {
                await selectedCommandBinding.InvokeAsync();
                _previousKeys.Clear();
            }
            else
            {
                Console.ResetColor();
                int commandToDisplay = 0;
                var possibleCommands = _commandBindings.Where(c => AreKeysEqual(c.Keys[0], keyinfo)).ToList();

                if (possibleCommands.Count == 0)
                {
                    HandleNoCommandFound();
                }
                else
                {
                    foreach (var commandBinding in possibleCommands)
                    {
                        Console.SetCursorPosition(10, Console.WindowHeight - 1 - possibleCommands.Count + commandToDisplay++);
                        _coloredConsoleRenderer.Write(
                            $"{{0,-{Console.WindowWidth - 10}}}",
                            string.Concat(commandBinding.Keys.Select(k => DisplayKey(k).Map(s => s.Length > 1 ? $" {s} " : s))) + ": " + commandBinding.Name
                        );
                    }
                }
                return false;
            }
            return true;

            void HandleNoCommandFound()
            {
                Console.SetCursorPosition(10, Console.WindowHeight - 2);
                _coloredConsoleRenderer.Write(
                    $"{{0,-{Console.WindowWidth - 10}}}",
                    "No command found for key(s): " + string.Format("{0,-20}", string.Concat(_previousKeys.Select(k => DisplayKey(k).Map(s => s.Length > 1 ? $" {s} " : s))))
                );
                _previousKeys.Clear();
            }
        }

        private static string DisplayKey(ConsoleKeyInfo keyInfo) =>
            string.IsNullOrWhiteSpace(keyInfo.KeyChar.ToString()) || keyInfo.KeyChar == '\0'
            ? keyInfo.Key.ToString()
            : keyInfo.KeyChar.ToString();

        private static bool AreKeysEqual(IReadOnlyList<ConsoleKeyInfo> collection1, IReadOnlyList<ConsoleKeyInfo> collection2)
        {
            if (collection1.Count != collection2.Count) return false;

            for (var i = 0; i < collection1.Count; i++)
            {
                if (!AreKeysEqual(collection1[i], collection2[i])) return false;
            }

            return true;
        }

        private static bool AreKeysEqual(ConsoleKeyInfo keyInfo1, ConsoleKeyInfo keyInfo2) =>
            keyInfo1.Key == keyInfo2.Key && keyInfo1.Modifiers == keyInfo2.Modifiers;

        public void MoveToIOLine(int left = 0)
        {
            Console.SetCursorPosition(left, Console.WindowHeight - 2);
        }
    }
}