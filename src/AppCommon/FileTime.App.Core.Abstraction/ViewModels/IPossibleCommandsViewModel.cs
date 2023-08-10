using System.Collections.ObjectModel;

namespace FileTime.App.Core.ViewModels;

public interface IPossibleCommandsViewModel
{
    ObservableCollection<IPossibleCommandEntryViewModel> PossibleCommands { get; }
}