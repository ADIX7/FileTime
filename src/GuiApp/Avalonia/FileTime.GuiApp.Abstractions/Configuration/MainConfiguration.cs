using Avalonia.Input;
using FileTime.App.Core.UserCommand;

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
            //new CommandBindingConfiguration(ConfigCommand.AutoRefresh, new KeyConfig(Key.R, shift: true)),
            //new CommandBindingConfiguration(ConfigCommand.ChangeTimelineMode, new[] { Key.T, Key.M }),
            new CommandBindingConfiguration(CloseTabCommand.CommandName, Key.Q),
            //new CommandBindingConfiguration(ConfigCommand.Compress, new[] { Key.Y, Key.C }),
            new CommandBindingConfiguration(CopyCommand.CommandName, new[] { Key.Y, Key.Y }),
            //new CommandBindingConfiguration(ConfigCommand.CopyHash, new[] { Key.C, Key.H }),
            //new CommandBindingConfiguration(ConfigCommand.CopyPath, new[] { Key.C, Key.P }),
            new CommandBindingConfiguration(CreateContainer.CommandName, Key.F7),
            new CommandBindingConfiguration(CreateContainer.CommandName, new[] { Key.C, Key.C }),
            new CommandBindingConfiguration(CreateElement.CommandName, new[] { Key.C, Key.E }),
            //new CommandBindingConfiguration(ConfigCommand.Cut, new[] { Key.D, Key.D }),
            //new CommandBindingConfiguration(ConfigCommand.Edit, new KeyConfig(Key.F4)),
            new CommandBindingConfiguration(EnterRapidTravelCommand.CommandName, new KeyConfig(Key.OemComma, shift: true)),
            //new CommandBindingConfiguration(ConfigCommand.FindByName, new[] { Key.F, Key.N }),
            //new CommandBindingConfiguration(ConfigCommand.FindByNameRegex, new[] { Key.F, Key.R }),
            //new CommandBindingConfiguration(ConfigCommand.GoToHome, new[] { Key.G, Key.H }),
            //new CommandBindingConfiguration(ConfigCommand.GoToPath, new KeyConfig(Key.L, ctrl: true)),
            //new CommandBindingConfiguration(ConfigCommand.GoToPath, new[] { Key.G, Key.P }),
            //new CommandBindingConfiguration(ConfigCommand.GoToProvider, new[] { Key.G, Key.T }),
            //new CommandBindingConfiguration(ConfigCommand.GoToRoot, new[] { Key.G, Key.R }),
            //new CommandBindingConfiguration(ConfigCommand.HardDelete, new[] { new KeyConfig(Key.D,shift: true), new KeyConfig(Key.D, shift: true) }),
            new CommandBindingConfiguration(MarkCommand.CommandName, Key.Space),
            //new CommandBindingConfiguration(ConfigCommand.MoveToLast, new KeyConfig(Key.G, shift: true)),
            //new CommandBindingConfiguration(ConfigCommand.MoveToFirst, new[] { Key.G, Key.G }),
            //new CommandBindingConfiguration(ConfigCommand.NextTimelineBlock, Key.L ),
            //new CommandBindingConfiguration(ConfigCommand.NextTimelineCommand, Key.J ),
            //new CommandBindingConfiguration(ConfigCommand.OpenInFileBrowser, new[] { Key.O, Key.E }),
            new CommandBindingConfiguration(PasteCommand.PasteMergeCommandName, new[] { Key.P, Key.P }),
            new CommandBindingConfiguration(PasteCommand.PasteOverwriteCommandName, new[] { Key.P, Key.O }),
            new CommandBindingConfiguration(PasteCommand.PasteSkipCommandName, new[] { Key.P, Key.S }),
            //new CommandBindingConfiguration(ConfigCommand.PinFavorite, new[] { Key.F, Key.P }),
            //new CommandBindingConfiguration(ConfigCommand.PreviousTimelineBlock, Key.H ),
            //new CommandBindingConfiguration(ConfigCommand.PreviousTimelineCommand, Key.K ),
            new CommandBindingConfiguration(RefreshCommand.CommandName, Key.R),
            //new CommandBindingConfiguration(ConfigCommand.Rename, Key.F2),
            //new CommandBindingConfiguration(ConfigCommand.Rename, new[] { Key.C, Key.W }),
            //new CommandBindingConfiguration(ConfigCommand.RunCommand, new KeyConfig(Key.D4, shift: true)),
            //new CommandBindingConfiguration(ConfigCommand.ScanContainerSize, new[] { Key.C, Key.S }),
            //new CommandBindingConfiguration(ConfigCommand.ShowAllShortcut, Key.F1),
            //new CommandBindingConfiguration(ConfigCommand.SoftDelete, new[] { new KeyConfig(Key.D), new KeyConfig(Key.D, shift: true) }),
            new CommandBindingConfiguration(SwitchToTabCommand.SwitchToLastTabCommandName, Key.D9),
            new CommandBindingConfiguration(SwitchToTabCommand.SwitchToTab1CommandName, Key.D1),
            new CommandBindingConfiguration(SwitchToTabCommand.SwitchToTab2CommandName, Key.D2),
            new CommandBindingConfiguration(SwitchToTabCommand.SwitchToTab3CommandName, Key.D3),
            new CommandBindingConfiguration(SwitchToTabCommand.SwitchToTab4CommandName, Key.D4),
            new CommandBindingConfiguration(SwitchToTabCommand.SwitchToTab5CommandName, Key.D5),
            new CommandBindingConfiguration(SwitchToTabCommand.SwitchToTab6CommandName, Key.D6),
            new CommandBindingConfiguration(SwitchToTabCommand.SwitchToTab7CommandName, Key.D7),
            new CommandBindingConfiguration(SwitchToTabCommand.SwitchToTab8CommandName, Key.D8),
            //new CommandBindingConfiguration(ConfigCommand.TimelinePause, new[] { Key.T, Key.P }),
            //new CommandBindingConfiguration(ConfigCommand.TimelineRefresh, new[] { Key.T, Key.R }),
            //new CommandBindingConfiguration(ConfigCommand.TimelineStart, new[] { Key.T, Key.S }),
            //new CommandBindingConfiguration(ConfigCommand.ToggleAdvancedIcons, new[] { Key.Z, Key.I }),
            new CommandBindingConfiguration(GoUpCommand.CommandName, Key.Left),
            new CommandBindingConfiguration(OpenSelectedCommand.CommandName, Key.Right),
            //new CommandBindingConfiguration(ConfigCommand.OpenOrRun, Key.Enter),
            new CommandBindingConfiguration(MoveCursorUpCommand.CommandName, Key.Up),
            new CommandBindingConfiguration(MoveCursorDownCommand.CommandName, Key.Down),
            //new CommandBindingConfiguration(ConfigCommand.MoveCursorUpPage, Key.PageUp),
            //new CommandBindingConfiguration(ConfigCommand.MoveCursorDownPage, Key.PageDown),
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