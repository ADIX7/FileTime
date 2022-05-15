using FileTime.GuiApp.Configuration;

namespace FileTime.GuiApp.Services;

public interface IKeyboardConfigurationService
{
    IReadOnlyDictionary<string, CommandBindingConfiguration> CommandBindings { get; }
    IReadOnlyDictionary<string, CommandBindingConfiguration> UniversalCommandBindings { get; }
    IReadOnlyDictionary<string, CommandBindingConfiguration> AllShortcut { get; }
}