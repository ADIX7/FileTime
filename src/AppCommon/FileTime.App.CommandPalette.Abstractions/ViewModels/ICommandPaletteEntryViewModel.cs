namespace FileTime.App.CommandPalette.ViewModels;

public interface ICommandPaletteEntryViewModel
{
    string Identifier { get; }
    string Title { get; }
    string Shortcuts { get; }
}