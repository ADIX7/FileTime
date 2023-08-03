using FileTime.GuiApp.App.Configuration;

namespace FileTime.GuiApp.App.Services;

public interface IKeyboardConfigurationService
{
    IReadOnlyList<CommandBindingConfiguration> CommandBindings { get; }
    IReadOnlyList<CommandBindingConfiguration> UniversalCommandBindings { get; }
    IReadOnlyList<CommandBindingConfiguration> AllShortcut { get; }
}