using MvvmGen;

namespace FileTime.App.CommandPalette.ViewModels;

[ViewModel]
public partial class CommandPaletteEntryViewModel : ICommandPaletteEntryViewModel
{
    [Property] private string _identifier;
    [Property] private string _title;

    public CommandPaletteEntryViewModel(string identifier, string title)
    {
        _identifier = identifier;
        _title = title;
    }
}