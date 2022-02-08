using Avalonia.Input;
using FileTime.App.Core.Command;
using System;
using System.Collections.Generic;

namespace FileTime.Avalonia.Configuration
{
    public static class MainConfiguration
    {
        private static readonly Lazy<List<CommandBindingConfiguration>> _defaultKeybindings = new(InitDefaultKeyBindings);
        internal const string KeybindingBaseConfigKey = "KeyBindings";

        public static Dictionary<string, string> Configuration { get; }

        static MainConfiguration()
        {
            Configuration = new();
            PopulateDefaultKeyBindings(Configuration, _defaultKeybindings.Value, KeybindingBaseConfigKey + ":" + nameof(KeyBindingConfiguration.DefaultKeyBindings));
        }

        private static void PopulateDefaultKeyBindings(Dictionary<string, string> keybindings, List<CommandBindingConfiguration> commandBindingConfigs, string basePath)
        {
            for (var i = 0; i < commandBindingConfigs.Count; i++)
            {
                var baseKey = basePath + $":[{i}]:";
                var commandBindingConfig = commandBindingConfigs[i];
                keybindings.Add(baseKey + nameof(CommandBindingConfiguration.Command), commandBindingConfig.Command.ToString());

                for (var j = 0; j < commandBindingConfig.Keys.Count; j++)
                {
                    var key = commandBindingConfig.Keys[j];
                    var keyBaseKey = baseKey + $"keys:[{j}]:";
                    keybindings.Add(keyBaseKey + nameof(KeyConfig.Key), key.Key.ToString());
                    keybindings.Add(keyBaseKey + nameof(KeyConfig.Shift), key.Shift.ToString());
                    keybindings.Add(keyBaseKey + nameof(KeyConfig.Alt), key.Alt.ToString());
                    keybindings.Add(keyBaseKey + nameof(KeyConfig.Ctrl), key.Ctrl.ToString());
                }
            }
        }

        private static List<CommandBindingConfiguration> InitDefaultKeyBindings()
        {
            return new List<CommandBindingConfiguration>()
            {
                new CommandBindingConfiguration(Commands.AutoRefresh, new KeyConfig(Key.R, shift: true)),
                new CommandBindingConfiguration(Commands.ChangeTimelineMode, new[] { Key.T, Key.M }),
                new CommandBindingConfiguration(Commands.CloseTab, Key.Q),
                new CommandBindingConfiguration(Commands.Copy, new[] { Key.Y, Key.Y }),
                new CommandBindingConfiguration(Commands.CopyPath, new[] { Key.C, Key.P }),
                new CommandBindingConfiguration(Commands.CreateContainer, Key.F7),
                new CommandBindingConfiguration(Commands.CreateContainer, new[] { Key.C, Key.C }),
                new CommandBindingConfiguration(Commands.CreateElement, new[] { Key.C, Key.E }),
                new CommandBindingConfiguration(Commands.Cut, new[] { Key.D, Key.D }),
                new CommandBindingConfiguration(Commands.EnterRapidTravel, new KeyConfig(Key.OemComma, shift: true)),
                new CommandBindingConfiguration(Commands.GoToHome, new[] { Key.G, Key.H }),
                new CommandBindingConfiguration(Commands.GoToPath, new KeyConfig(Key.OemComma, ctrl: true)),
                new CommandBindingConfiguration(Commands.GoToPath, new[] { Key.G, Key.P }),
                new CommandBindingConfiguration(Commands.GoToProvider, new[] { Key.G, Key.T }),
                new CommandBindingConfiguration(Commands.GoToRoot, new[] { Key.G, Key.R }),
                new CommandBindingConfiguration(Commands.HardDelete, new[] { new KeyConfig(Key.D,shift: true), new KeyConfig(Key.D, shift: true) }),
                new CommandBindingConfiguration(Commands.Mark, Key.Space),
                new CommandBindingConfiguration(Commands.MoveToLast, new KeyConfig(Key.G, shift: true)),
                new CommandBindingConfiguration(Commands.MoveToFirst, new[] { Key.G, Key.G }),
                new CommandBindingConfiguration(Commands.NextTimelineBlock, Key.L ),
                new CommandBindingConfiguration(Commands.NextTimelineCommand, Key.J ),
                new CommandBindingConfiguration(Commands.OpenInFileBrowser, new[] { Key.O, Key.E }),
                new CommandBindingConfiguration(Commands.PasteMerge, new[] { Key.P, Key.P }),
                new CommandBindingConfiguration(Commands.PasteOverwrite, new[] { Key.P, Key.O }),
                new CommandBindingConfiguration(Commands.PasteSkip, new[] { Key.P, Key.S }),
                new CommandBindingConfiguration(Commands.PreviousTimelineBlock, Key.H ),
                new CommandBindingConfiguration(Commands.PreviousTimelineCommand, Key.K ),
                new CommandBindingConfiguration(Commands.Refresh, Key.R),
                new CommandBindingConfiguration(Commands.Rename, Key.F2),
                new CommandBindingConfiguration(Commands.Rename, new[] { Key.C, Key.W }),
                new CommandBindingConfiguration(Commands.RunCommand, new KeyConfig(Key.D4, shift: true)),
                new CommandBindingConfiguration(Commands.ShowAllShotcut, Key.F1),
                new CommandBindingConfiguration(Commands.SoftDelete, new[] { new KeyConfig(Key.D), new KeyConfig(Key.D, shift: true) }),
                new CommandBindingConfiguration(Commands.SwitchToLastTab, Key.D9),
                new CommandBindingConfiguration(Commands.SwitchToTab1, Key.D1),
                new CommandBindingConfiguration(Commands.SwitchToTab2, Key.D2),
                new CommandBindingConfiguration(Commands.SwitchToTab3, Key.D3),
                new CommandBindingConfiguration(Commands.SwitchToTab4, Key.D4),
                new CommandBindingConfiguration(Commands.SwitchToTab5, Key.D5),
                new CommandBindingConfiguration(Commands.SwitchToTab6, Key.D6),
                new CommandBindingConfiguration(Commands.SwitchToTab7, Key.D7),
                new CommandBindingConfiguration(Commands.SwitchToTab8, Key.D8),
                new CommandBindingConfiguration(Commands.TimelinePause, new[] { Key.T, Key.P }),
                new CommandBindingConfiguration(Commands.TimelineRefresh, new[] { Key.T, Key.R }),
                new CommandBindingConfiguration(Commands.TimelineStart, new[] { Key.T, Key.S }),
                new CommandBindingConfiguration(Commands.ToggleAdvancedIcons, new[] { Key.Z, Key.I }),
                new CommandBindingConfiguration(Commands.GoUp, Key.Left),
                new CommandBindingConfiguration(Commands.Open, Key.Right),
                new CommandBindingConfiguration(Commands.OpenOrRun, Key.Enter),
                new CommandBindingConfiguration(Commands.MoveCursorUp, Key.Up),
                new CommandBindingConfiguration(Commands.MoveCursorDown, Key.Down),
                new CommandBindingConfiguration(Commands.MoveCursorUpPage, Key.PageUp),
                new CommandBindingConfiguration(Commands.MoveCursorDownPage, Key.PageDown),
            };
        }
    }
}