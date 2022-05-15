using System.Collections.ObjectModel;
using FileTime.GuiApp.Configuration;
using Microsoft.Extensions.Options;

namespace FileTime.GuiApp.Services;

public class KeyboardConfigurationService : IKeyboardConfigurationService
{
    public IReadOnlyDictionary<string, CommandBindingConfiguration> CommandBindings { get; }
    public IReadOnlyDictionary<string, CommandBindingConfiguration> UniversalCommandBindings { get; }
    public IReadOnlyDictionary<string, CommandBindingConfiguration> AllShortcut { get; }

    public KeyboardConfigurationService(IOptions<KeyBindingConfiguration> keyBindingConfiguration)
    {
        var commandBindings = new Dictionary<string, CommandBindingConfiguration>();
        var universalCommandBindings = new Dictionary<string, CommandBindingConfiguration>();
        IEnumerable<CommandBindingConfiguration> keyBindings = keyBindingConfiguration.Value.KeyBindings;

        if (keyBindingConfiguration.Value.UseDefaultBindings)
        {
            keyBindings = keyBindings.Concat(keyBindingConfiguration.Value.DefaultKeyBindings);
        }

        foreach (var keyBinding in keyBindings)
        {
            if (string.IsNullOrWhiteSpace(keyBinding.Command))
            {
                throw new FormatException($"No command is set in keybinding for keys '{keyBinding.KeysDisplayText}'");
            }
            else if (keyBinding.Keys.Count == 0)
            {
                throw new FormatException($"No keys set in keybinding for command '{keyBinding.Command}'.");
            }

            if (IsUniversal(keyBinding))
            {
                universalCommandBindings.Add(keyBinding.Command, keyBinding);
            }
            else
            {
                commandBindings.Add(keyBinding.Command, keyBinding);
            }
        }

        CommandBindings = new ReadOnlyDictionary<string, CommandBindingConfiguration>(commandBindings);
        UniversalCommandBindings = new ReadOnlyDictionary<string, CommandBindingConfiguration>(universalCommandBindings);
        AllShortcut = new ReadOnlyDictionary<string, CommandBindingConfiguration>(new Dictionary<string, CommandBindingConfiguration>(CommandBindings.Concat(UniversalCommandBindings)));
    }

    private static bool IsUniversal(CommandBindingConfiguration keyMapping)
    {
        return false;
        //return keyMapping.Command is ConfigCommand.GoUp or ConfigCommand.Open or ConfigCommand.OpenOrRun or ConfigCommand.MoveCursorUp or ConfigCommand.MoveCursorDown or ConfigCommand.MoveCursorUpPage or ConfigCommand.MoveCursorDownPage;
    }
}