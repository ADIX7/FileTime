using Avalonia.Input;
using FileTime.App.Core.ViewModels;

namespace FileTime.App.CommandPalette.ViewModels;

public interface ICommandPaletteViewModel : IModalViewModel
{
    IObservable<bool> ShowWindow { get; }
    List<ICommandPaletteEntryViewModel> FilteredMatches { get; }
    string SearchText { get; set; }
    ICommandPaletteEntryViewModel SelectedItem { get; set; }
    void Close();
    void HandleKeyDown(KeyEventArgs keyEventArgs);
}