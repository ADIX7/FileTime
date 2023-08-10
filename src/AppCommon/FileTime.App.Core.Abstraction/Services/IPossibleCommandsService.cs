using System.Collections.ObjectModel;
using FileTime.App.Core.Configuration;

namespace FileTime.App.Core.Services;

public interface IPossibleCommandsService
{
    ReadOnlyObservableCollection<CommandBindingConfiguration> PossibleCommands { get; set; }
    void Clear();
    void Add(CommandBindingConfiguration commandBindingConfiguration);
    void Remove(CommandBindingConfiguration commandBindingConfiguration);
    void AddRange(IEnumerable<CommandBindingConfiguration> commandBindingConfigurations);
}