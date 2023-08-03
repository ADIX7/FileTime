using FileTime.App.Core.UserCommand;
using FileTime.GuiApp.App.Configuration;
using Microsoft.Extensions.Options;

namespace FileTime.GuiApp.App.Services;

public class KeyboardConfigurationService : IKeyboardConfigurationService
{
    public IReadOnlyList<CommandBindingConfiguration> CommandBindings { get; }
    public IReadOnlyList<CommandBindingConfiguration> UniversalCommandBindings { get; }
    public IReadOnlyList<CommandBindingConfiguration> AllShortcut { get; }

    public KeyboardConfigurationService(IOptions<KeyBindingConfiguration> keyBindingConfiguration)
    {
        IEnumerable<CommandBindingConfiguration> keyBindings = keyBindingConfiguration.Value.KeyBindings;

        if (keyBindingConfiguration.Value.UseDefaultBindings)
        {
            keyBindings = keyBindings.Concat(keyBindingConfiguration.Value.DefaultKeyBindings);
        }

        var commandBindings = new List<CommandBindingConfiguration>();
        var universalCommandBindings = new List<CommandBindingConfiguration>();
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
                universalCommandBindings.Add(keyBinding);
            }
            else
            {
                commandBindings.Add(keyBinding);
            }
        }

        CommandBindings = commandBindings.AsReadOnly();
        UniversalCommandBindings = universalCommandBindings.AsReadOnly();
        AllShortcut = new List<CommandBindingConfiguration>(UniversalCommandBindings.Concat(CommandBindings)).AsReadOnly();
    }

    private static bool IsUniversal(CommandBindingConfiguration keyMapping)
    {
        return keyMapping.Command is
            GoUpCommand.CommandName
            or OpenSelectedCommand.CommandName
            or MoveCursorDownCommand.CommandName
            or MoveCursorDownPageCommand.CommandName
            or MoveCursorUpCommand.CommandName
            or MoveCursorUpPageCommand.CommandName;
    }
}