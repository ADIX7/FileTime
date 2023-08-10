namespace FileTime.App.Core.ViewModels;

public record PossibleCommandEntryViewModel(
    string CommandName, 
    string Title, 
    string KeysText) : IPossibleCommandEntryViewModel;