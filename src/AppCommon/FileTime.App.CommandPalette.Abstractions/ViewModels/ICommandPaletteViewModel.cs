using Avalonia.Input;
using FileTime.App.Core.ViewModels;
using FileTime.App.FuzzyPanel;

namespace FileTime.App.CommandPalette.ViewModels;

public interface ICommandPaletteViewModel : IFuzzyPanelViewModel<ICommandPaletteEntryViewModel>, IModalViewModel
{
    IObservable<bool> ShowWindow { get; }
    void Close();
    Task<bool> HandleKeyUp(KeyEventArgs keyEventArgs);
}