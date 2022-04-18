using FileTime.App.Core.Command;
using FileTime.GuiApp.Configuration;
using Microsoft.Extensions.Options;

namespace FileTime.GuiApp.Services
{
    public class KeyboardConfigurationService : IKeyboardConfigurationService
    {
        public IReadOnlyList<CommandBindingConfiguration> CommandBindings { get; }
        public IReadOnlyList<CommandBindingConfiguration> UniversalCommandBindings { get; }
        public IReadOnlyList<CommandBindingConfiguration> AllShortcut { get; }

        public KeyboardConfigurationService(IOptions<KeyBindingConfiguration> keyBindingConfiguration)
        {
            var commandBindings = new List<CommandBindingConfiguration>();
            var universalCommandBindings = new List<CommandBindingConfiguration>();
            IEnumerable<CommandBindingConfiguration> keyBindings = keyBindingConfiguration.Value.KeyBindings;

            if (keyBindingConfiguration.Value.UseDefaultBindings)
            {
                keyBindings = keyBindings.Concat(keyBindingConfiguration.Value.DefaultKeyBindings);
            }

            foreach (var keyBinding in keyBindings)
            {
                if (keyBinding.Command == Commands.None)
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
            AllShortcut = new List<CommandBindingConfiguration>(CommandBindings.Concat(UniversalCommandBindings)).AsReadOnly();
        }

        private static bool IsUniversal(CommandBindingConfiguration keyMapping)
        {
            return keyMapping.Command == Commands.GoUp
                || keyMapping.Command == Commands.Open
                || keyMapping.Command == Commands.OpenOrRun
                || keyMapping.Command == Commands.MoveCursorUp
                || keyMapping.Command == Commands.MoveCursorDown
                || keyMapping.Command == Commands.MoveCursorUpPage
                || keyMapping.Command == Commands.MoveCursorDownPage;
        }
    }
}