namespace FileTime.App.CommandPalette.Models;

public interface ICommandPaletteEntry
{
    string Identifier { get; }
    string Title { get; }
    
}