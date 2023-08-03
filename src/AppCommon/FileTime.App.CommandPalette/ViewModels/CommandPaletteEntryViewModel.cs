namespace FileTime.App.CommandPalette.ViewModels;

public record CommandPaletteEntryViewModel(string Identifier, string Title, string Shortcuts) : ICommandPaletteEntryViewModel;