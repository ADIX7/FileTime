using Avalonia.Input;
using FileTime.App.Core.Command;
using System;
using System.Collections.Generic;

namespace FileTime.GuiApp.Configuration;

public static class MainConfiguration
{
    private static readonly Lazy<List<CommandBindingConfiguration>> _defaultKeybindings = new(InitDefaultKeyBindings);

    public static Dictionary<string, string> Configuration { get; }

    static MainConfiguration()
    {
        Configuration = new();
        PopulateDefaultEditorPrograms(Configuration);
        PopulateDefaultKeyBindings(Configuration, _defaultKeybindings.Value, SectionNames.KeybindingSectionName + ":" + nameof(KeyBindingConfiguration.DefaultKeyBindings));
    }

    private static void PopulateDefaultKeyBindings(Dictionary<string, string> configuration, List<CommandBindingConfiguration> commandBindingConfigs, string basePath)
    {
        for (var i = 0; i < commandBindingConfigs.Count; i++)
        {
            var baseKey = basePath + $":[{i}]:";
            var commandBindingConfig = commandBindingConfigs[i];
            configuration.Add(baseKey + nameof(CommandBindingConfiguration.Command), commandBindingConfig.Command.ToString());

            for (var j = 0; j < commandBindingConfig.Keys.Count; j++)
            {
                var key = commandBindingConfig.Keys[j];
                var keyBaseKey = baseKey + $"keys:[{j}]:";
                configuration.Add(keyBaseKey + nameof(KeyConfig.Key), key.Key.ToString());
                configuration.Add(keyBaseKey + nameof(KeyConfig.Shift), key.Shift.ToString());
                configuration.Add(keyBaseKey + nameof(KeyConfig.Alt), key.Alt.ToString());
                configuration.Add(keyBaseKey + nameof(KeyConfig.Ctrl), key.Ctrl.ToString());
            }
        }
    }

    private static List<CommandBindingConfiguration> InitDefaultKeyBindings()
    {
        return new List<CommandBindingConfiguration>()
        {
            new CommandBindingConfiguration(Command.AutoRefresh, new KeyConfig(Key.R, shift: true)),
            new CommandBindingConfiguration(Command.ChangeTimelineMode, new[] { Key.T, Key.M }),
            new CommandBindingConfiguration(Command.CloseTab, Key.Q),
            new CommandBindingConfiguration(Command.Compress, new[] { Key.Y, Key.C }),
            new CommandBindingConfiguration(Command.Copy, new[] { Key.Y, Key.Y }),
            new CommandBindingConfiguration(Command.CopyHash, new[] { Key.C, Key.H }),
            new CommandBindingConfiguration(Command.CopyPath, new[] { Key.C, Key.P }),
            new CommandBindingConfiguration(Command.CreateContainer, Key.F7),
            new CommandBindingConfiguration(Command.CreateContainer, new[] { Key.C, Key.C }),
            new CommandBindingConfiguration(Command.CreateElement, new[] { Key.C, Key.E }),
            new CommandBindingConfiguration(Command.Cut, new[] { Key.D, Key.D }),
            new CommandBindingConfiguration(Command.Edit, new KeyConfig(Key.F4)),
            new CommandBindingConfiguration(Command.EnterRapidTravel, new KeyConfig(Key.OemComma, shift: true)),
            new CommandBindingConfiguration(Command.FindByName, new[] { Key.F, Key.N }),
            new CommandBindingConfiguration(Command.FindByNameRegex, new[] { Key.F, Key.R }),
            new CommandBindingConfiguration(Command.GoToHome, new[] { Key.G, Key.H }),
            new CommandBindingConfiguration(Command.GoToPath, new KeyConfig(Key.L, ctrl: true)),
            new CommandBindingConfiguration(Command.GoToPath, new[] { Key.G, Key.P }),
            new CommandBindingConfiguration(Command.GoToProvider, new[] { Key.G, Key.T }),
            new CommandBindingConfiguration(Command.GoToRoot, new[] { Key.G, Key.R }),
            new CommandBindingConfiguration(Command.HardDelete, new[] { new KeyConfig(Key.D,shift: true), new KeyConfig(Key.D, shift: true) }),
            new CommandBindingConfiguration(Command.Mark, Key.Space),
            new CommandBindingConfiguration(Command.MoveToLast, new KeyConfig(Key.G, shift: true)),
            new CommandBindingConfiguration(Command.MoveToFirst, new[] { Key.G, Key.G }),
            new CommandBindingConfiguration(Command.NextTimelineBlock, Key.L ),
            new CommandBindingConfiguration(Command.NextTimelineCommand, Key.J ),
            new CommandBindingConfiguration(Command.OpenInFileBrowser, new[] { Key.O, Key.E }),
            new CommandBindingConfiguration(Command.PasteMerge, new[] { Key.P, Key.P }),
            new CommandBindingConfiguration(Command.PasteOverwrite, new[] { Key.P, Key.O }),
            new CommandBindingConfiguration(Command.PasteSkip, new[] { Key.P, Key.S }),
            new CommandBindingConfiguration(Command.PinFavorite, new[] { Key.F, Key.P }),
            new CommandBindingConfiguration(Command.PreviousTimelineBlock, Key.H ),
            new CommandBindingConfiguration(Command.PreviousTimelineCommand, Key.K ),
            new CommandBindingConfiguration(Command.Refresh, Key.R),
            new CommandBindingConfiguration(Command.Rename, Key.F2),
            new CommandBindingConfiguration(Command.Rename, new[] { Key.C, Key.W }),
            new CommandBindingConfiguration(Command.RunCommand, new KeyConfig(Key.D4, shift: true)),
            new CommandBindingConfiguration(Command.ScanContainerSize, new[] { Key.C, Key.S }),
            new CommandBindingConfiguration(Command.ShowAllShotcut, Key.F1),
            new CommandBindingConfiguration(Command.SoftDelete, new[] { new KeyConfig(Key.D), new KeyConfig(Key.D, shift: true) }),
            new CommandBindingConfiguration(Command.SwitchToLastTab, Key.D9),
            new CommandBindingConfiguration(Command.SwitchToTab1, Key.D1),
            new CommandBindingConfiguration(Command.SwitchToTab2, Key.D2),
            new CommandBindingConfiguration(Command.SwitchToTab3, Key.D3),
            new CommandBindingConfiguration(Command.SwitchToTab4, Key.D4),
            new CommandBindingConfiguration(Command.SwitchToTab5, Key.D5),
            new CommandBindingConfiguration(Command.SwitchToTab6, Key.D6),
            new CommandBindingConfiguration(Command.SwitchToTab7, Key.D7),
            new CommandBindingConfiguration(Command.SwitchToTab8, Key.D8),
            new CommandBindingConfiguration(Command.TimelinePause, new[] { Key.T, Key.P }),
            new CommandBindingConfiguration(Command.TimelineRefresh, new[] { Key.T, Key.R }),
            new CommandBindingConfiguration(Command.TimelineStart, new[] { Key.T, Key.S }),
            new CommandBindingConfiguration(Command.ToggleAdvancedIcons, new[] { Key.Z, Key.I }),
            new CommandBindingConfiguration(Command.GoUp, Key.Left),
            new CommandBindingConfiguration(Command.Open, Key.Right),
            new CommandBindingConfiguration(Command.OpenOrRun, Key.Enter),
            new CommandBindingConfiguration(Command.MoveCursorUp, Key.Up),
            new CommandBindingConfiguration(Command.MoveCursorDown, Key.Down),
            new CommandBindingConfiguration(Command.MoveCursorUpPage, Key.PageUp),
            new CommandBindingConfiguration(Command.MoveCursorDownPage, Key.PageDown),
        };
    }

    private static void PopulateDefaultEditorPrograms(Dictionary<string, string> configuration)
    {
        var editorPrograms = new List<ProgramConfiguration>()
        {
            new ProgramConfiguration(@"c:\Program Files\Notepad++\notepad++.exe"),
            new ProgramConfiguration("notepad.exe"),
        };

        for (var i = 0; i < editorPrograms.Count; i++)
        {
            if (editorPrograms[i].Path is not string path) continue;
            configuration.Add($"{SectionNames.ProgramsSectionName}:{nameof(ProgramsConfiguration.DefaultEditorPrograms)}:[{i}]:{nameof(ProgramConfiguration.Path)}", path);

            if (editorPrograms[i].Arguments is string arguments)
            {
                configuration.Add($"{SectionNames.ProgramsSectionName}:{nameof(ProgramsConfiguration.DefaultEditorPrograms)}:[{i}]:{nameof(ProgramConfiguration.Arguments)}", arguments);
            }
        }
    }
}