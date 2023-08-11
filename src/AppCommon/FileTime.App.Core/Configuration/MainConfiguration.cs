using FileTime.App.Core.Models;
using FileTime.App.Core.UserCommand;
using FileTime.Providers.LocalAdmin;
using GeneralInputKey;

namespace FileTime.App.Core.Configuration;

public class MainConfiguration
{
    private static readonly Lazy<List<CommandBindingConfiguration>> _defaultKeybindings = new(InitDefaultKeyBindings);

    public static Dictionary<string, string?> Configuration { get; }

    static MainConfiguration()
    {
        Configuration = new()
        {
            {AdminElevationConfiguration.SectionName + ":" + nameof(AdminElevationConfiguration.ServerExecutablePath), "FileTime.Server.exe"},
        };

        PopulateDefaultEditorPrograms(Configuration);
        PopulateDefaultKeyBindings(Configuration, _defaultKeybindings.Value,
            SectionNames.KeybindingSectionName + ":" + nameof(KeyBindingConfiguration.DefaultKeyBindings));
    }

    private static void PopulateDefaultKeyBindings(Dictionary<string, string?> configuration,
        List<CommandBindingConfiguration> commandBindingConfigs, string basePath)
    {
        for (var i = 0; i < commandBindingConfigs.Count; i++)
        {
            var baseKey = basePath + $":[{i}]:";
            var commandBindingConfig = commandBindingConfigs[i];
            configuration.Add(baseKey + nameof(CommandBindingConfiguration.Command),
                commandBindingConfig.Command);

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

    private static List<CommandBindingConfiguration> InitDefaultKeyBindings() =>
        new List<CommandBindingConfiguration>
        {
            //new CommandBindingConfiguration(ConfigCommand.AutoRefresh, new KeyConfig(Keys.R, shift: true)),
            //new CommandBindingConfiguration(ConfigCommand.ChangeTimelineMode, new[] { Keys.T, Keys.M }),
            new(CloseTabCommand.CommandName, Keys.Q),
            //new CommandBindingConfiguration(ConfigCommand.Compress, new[] { Keys.Y, Keys.C }),
            new(CopyBase64Command.CommandName, new[] {Keys.C, Keys.B}),
            new(CopyCommand.CommandName, new[] {Keys.Y, Keys.Y}),
            //new CommandBindingConfiguration(ConfigCommand.CopyHash, new[] { Keys.C, Keys.H }),
            new(CopyNativePathCommand.CommandName, new[] {Keys.C, Keys.P}),
            new(CopyFilesToClipboardCommand.CommandName, new[] {Keys.Y, Keys.C}),
            new(CreateContainer.CommandName, Keys.F7),
            new(CreateContainer.CommandName, new[] {Keys.C, Keys.C}),
            new(CreateElementCommand.CommandName, new[] {Keys.C, Keys.E}),
            //new CommandBindingConfiguration(ConfigCommand.Cut, new[] { Keys.D, Keys.D }),
            //new CommandBindingConfiguration(ConfigCommand.Edit, new KeyConfig(Keys.F4)),
            new(EnterRapidTravelCommand.CommandName, new KeyConfig(Keys.Comma, shift: true)),
            new(EnterRapidTravelCommand.CommandName, new KeyConfig(Keys.Question, shift: true)),
            new(GoBackCommand.CommandName, new KeyConfig(Keys.Left, alt: true)),
            new(GoByFrequencyCommand.CommandName, Keys.Z),
            new(GoForwardCommand.CommandName, new KeyConfig(Keys.Right, alt: true)),
            new(GoToHomeCommand.CommandName, new[] {Keys.G, Keys.H}),
            new(GoToPathCommand.CommandName, new KeyConfig(Keys.L, ctrl: true)),
            new(GoToPathCommand.CommandName, new[] {Keys.G, Keys.P}),
            new(GoToProviderCommand.CommandName, new[] {Keys.G, Keys.T}),
            new(GoToRootCommand.CommandName, new[] {Keys.G, Keys.R}),
            new(GoUpCommand.CommandName, Keys.Left),
            new(DeleteCommand.HardDeleteCommandName, new[] {new KeyConfig(Keys.D, shift: true), new KeyConfig(Keys.D, shift: true)}),
            new(MarkCommand.CommandName, Keys.Space),
            new(MoveCursorToLastCommand.CommandName, new KeyConfig(Keys.G, shift: true)),
            new(MoveCursorToFirstCommand.CommandName, new[] {Keys.G, Keys.G}),
            new(MoveCursorUpCommand.CommandName, Keys.Up),
            new(MoveCursorDownCommand.CommandName, Keys.Down),
            new(MoveCursorUpPageCommand.CommandName, Keys.PageUp),
            new(MoveCursorDownPageCommand.CommandName, Keys.PageDown),
            //new CommandBindingConfiguration(ConfigCommand.NextTimelineBlock, Keys.L ),
            //new CommandBindingConfiguration(ConfigCommand.NextTimelineCommand, Keys.J ),
            new(OpenSelectedCommand.CommandName, Keys.Right),
            new(OpenCommandPaletteCommand.CommandName, new[] {Keys.F1}),
            new(OpenCommandPaletteCommand.CommandName, new[] {new KeyConfig(Keys.P, ctrl: true, shift: true)}),
            new(OpenInDefaultFileExplorerCommand.CommandName, new[] {Keys.O, Keys.E}),
            new(PasteCommand.PasteMergeCommandName, new[] {Keys.P, Keys.P}),
            new(PasteCommand.PasteOverwriteCommandName, new[] {Keys.P, Keys.O}),
            new(PasteCommand.PasteSkipCommandName, new[] {Keys.P, Keys.S}),
            new(PasteFilesFromClipboardCommand.PasteMergeCommandName, new[] {new KeyConfig(Keys.V, ctrl: true)}),
            new(PasteFilesFromClipboardCommand.PasteOverwriteCommandName, new[] {new KeyConfig(Keys.V, ctrl: true, shift: true)}),
            //new CommandBindingConfiguration(ConfigCommand.PinFavorite, new[] { Keys.F, Keys.P }),
            //new CommandBindingConfiguration(ConfigCommand.PreviousTimelineBlock, Keys.H ),
            //new CommandBindingConfiguration(ConfigCommand.PreviousTimelineCommand, Keys.K ),
            new(RefreshCommand.CommandName, Keys.R),
            new(RenameCommand.CommandName, Keys.F2),
            new(RenameCommand.CommandName, new[] {Keys.C, Keys.W}),
            new(IdentifiableRunOrOpenCommand.CommandName, Keys.Enter),
            //new CommandBindingConfiguration(ConfigCommand.RunCommand, new KeyConfig(Keys.D4, shift: true)),
            //new CommandBindingConfiguration(ConfigCommand.ScanContainerSize, new[] { Keys.C, Keys.S }),
            //new CommandBindingConfiguration(ConfigCommand.ShowAllShortcut, Keys.F1),
            new(DeleteCommand.SoftDeleteCommandName, new[] {new KeyConfig(Keys.D), new KeyConfig(Keys.D, shift: true)}),
            new(IdentifiableSearchCommand.SearchByNameContainsCommandName, new[] {Keys.S, Keys.N}),
            new(SwitchToTabCommand.SwitchToLastTabCommandName, new[] {new KeyConfig(Keys.Num9, alt: true)}),
            new(SwitchToTabCommand.SwitchToTab1CommandName, new[] {new KeyConfig(Keys.Num1, alt: true)}),
            new(SwitchToTabCommand.SwitchToTab2CommandName, new[] {new KeyConfig(Keys.Num2, alt: true)}),
            new(SwitchToTabCommand.SwitchToTab3CommandName, new[] {new KeyConfig(Keys.Num3, alt: true)}),
            new(SwitchToTabCommand.SwitchToTab4CommandName, new[] {new KeyConfig(Keys.Num4, alt: true)}),
            new(SwitchToTabCommand.SwitchToTab5CommandName, new[] {new KeyConfig(Keys.Num5, alt: true)}),
            new(SwitchToTabCommand.SwitchToTab6CommandName, new[] {new KeyConfig(Keys.Num6, alt: true)}),
            new(SwitchToTabCommand.SwitchToTab7CommandName, new[] {new KeyConfig(Keys.Num7, alt: true)}),
            new(SwitchToTabCommand.SwitchToTab8CommandName, new[] {new KeyConfig(Keys.Num8, alt: true)}),
            new(PauseCommandSchedulerCommand.CommandName, new[] {Keys.T, Keys.P}),
            //new CommandBindingConfiguration(ConfigCommand.TimelineRefresh, new[] { Keys.T, Keys.R }),
            new(StartCommandSchedulerCommand.CommandName, new[] {Keys.T, Keys.S}),
            //new CommandBindingConfiguration(ConfigCommand.ToggleAdvancedIcons, new[] { Keys.Z, Keys.I }),
        };

    private static void PopulateDefaultEditorPrograms(Dictionary<string, string?> configuration)
    {
        var editorPrograms = new List<ProgramConfiguration>()
        {
            new(@"c:\Program Files\Notepad++\notepad++.exe"),
            new("notepad.exe"),
        };

        for (var i = 0; i < editorPrograms.Count; i++)
        {
            if (editorPrograms[i].Path is not { } path) continue;
            configuration.Add(
                $"{SectionNames.ProgramsSectionName}:{nameof(ProgramsConfiguration.DefaultEditorPrograms)}:[{i}]:{nameof(ProgramConfiguration.Path)}",
                path);

            if (editorPrograms[i].Arguments is { } arguments)
            {
                configuration.Add(
                    $"{SectionNames.ProgramsSectionName}:{nameof(ProgramsConfiguration.DefaultEditorPrograms)}:[{i}]:{nameof(ProgramConfiguration.Arguments)}",
                    arguments);
            }
        }
    }
}