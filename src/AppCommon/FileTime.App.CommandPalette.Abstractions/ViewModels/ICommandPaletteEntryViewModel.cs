namespace FileTime.App.CommandPalette.ViewModels;

public interface ICommandPaletteEntryViewModel
{
    string Identifier { get; set; }
    string Title { get; set; }
}