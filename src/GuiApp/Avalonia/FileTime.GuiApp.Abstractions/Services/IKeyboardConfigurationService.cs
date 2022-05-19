using FileTime.GuiApp.Configuration;

namespace FileTime.GuiApp.Services;

public interface IKeyboardConfigurationService
{
    IReadOnlyList<CommandBindingConfiguration> CommandBindings { get; }
    IReadOnlyList<CommandBindingConfiguration> UniversalCommandBindings { get; }
    IReadOnlyList<CommandBindingConfiguration> AllShortcut { get; }
}