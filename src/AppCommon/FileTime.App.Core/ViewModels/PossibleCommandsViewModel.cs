using System.Collections.ObjectModel;
using FileTime.App.Core.Configuration;
using FileTime.App.Core.Services;
using ObservableComputations;

namespace FileTime.App.Core.ViewModels;

public class PossibleCommandsViewModel : IPossibleCommandsViewModel, IDisposable
{
    private readonly IIdentifiableUserCommandService _identifiableUserCommandService;
    private readonly OcConsumer _ocConsumer = new();
    public ObservableCollection<IPossibleCommandEntryViewModel> PossibleCommands { get; }

    public PossibleCommandsViewModel(
        IPossibleCommandsService possibleCommandsService,
        IIdentifiableUserCommandService identifiableUserCommandService)
    {
        _identifiableUserCommandService = identifiableUserCommandService;
        PossibleCommands = possibleCommandsService
            .PossibleCommands
            .Selecting(c => CreatePossibleCommandViewModel(c))
            .For(_ocConsumer);
    }
    
    private IPossibleCommandEntryViewModel CreatePossibleCommandViewModel(CommandBindingConfiguration commandBindingConfiguration)
    {
        var commandName = commandBindingConfiguration.Command;
        var title = _identifiableUserCommandService.GetCommand(commandName)?.Title ?? commandName;
        return new PossibleCommandEntryViewModel(
            CommandName: commandName,
            Title: title,
            KeysText: commandBindingConfiguration.GetKeysDisplayText());
    }

    public void Dispose() => _ocConsumer.Dispose();
}