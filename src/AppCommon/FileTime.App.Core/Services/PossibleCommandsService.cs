using System.Collections.ObjectModel;
using FileTime.App.Core.Configuration;

namespace FileTime.App.Core.Services;

public class PossibleCommandsService : IPossibleCommandsService
{
    private readonly ObservableCollection<CommandBindingConfiguration> _possibleCommands = new();
    public ReadOnlyObservableCollection<CommandBindingConfiguration> PossibleCommands { get; set; }

    public PossibleCommandsService()
    {
        PossibleCommands = new ReadOnlyObservableCollection<CommandBindingConfiguration>(_possibleCommands);
    }

    public void Clear() => _possibleCommands.Clear();

    public void Add(CommandBindingConfiguration commandBindingConfiguration)
        => _possibleCommands.Add(commandBindingConfiguration);

    public void AddRange(IEnumerable<CommandBindingConfiguration> commandBindingConfigurations)
    {
        foreach (var commandBindingConfiguration in commandBindingConfigurations)
        {
            _possibleCommands.Add(commandBindingConfiguration);
        }
    }

    public void Remove(CommandBindingConfiguration commandBindingConfiguration)
        => _possibleCommands.Remove(commandBindingConfiguration);
}