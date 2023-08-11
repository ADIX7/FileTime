using DeclarativeProperty;
using FileTime.App.Core.Models;
using FileTime.App.Core.ViewModels;
using FileTime.App.FuzzyPanel;
using GeneralInputKey;

namespace FileTime.App.CommandPalette.ViewModels;

public interface ICommandPaletteViewModel : IFuzzyPanelViewModel<ICommandPaletteEntryViewModel>, IModalViewModel
{
    IDeclarativeProperty<bool> ShowWindow { get; }
    void Close();
    Task<bool> HandleKeyUp(GeneralKeyEventArgs keyEventArgs);
}