namespace FileTime.App.CommandPalette.Models;

public class CommandPaletteEntry : ICommandPaletteEntry
{
    public string Identifier { get; }
    public string Title { get; }

    public CommandPaletteEntry(string identifier, string title)
    {
        Identifier = identifier;
        Title = title;
    }
}