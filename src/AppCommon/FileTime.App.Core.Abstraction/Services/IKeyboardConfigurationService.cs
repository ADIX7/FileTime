using FileTime.App.Core.Configuration;

namespace FileTime.App.Core.Services;

public interface IKeyboardConfigurationService
{
    IReadOnlyList<CommandBindingConfiguration> CommandBindings { get; }
    IReadOnlyList<CommandBindingConfiguration> UniversalCommandBindings { get; }
    IReadOnlyList<CommandBindingConfiguration> AllShortcut { get; }
}