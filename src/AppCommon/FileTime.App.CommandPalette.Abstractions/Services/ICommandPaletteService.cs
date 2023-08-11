using DeclarativeProperty;
using FileTime.App.CommandPalette.Models;
using FileTime.App.CommandPalette.ViewModels;

namespace FileTime.App.CommandPalette.Services;

public interface ICommandPaletteService
{
    IDeclarativeProperty<bool> ShowWindow { get; }
    void OpenCommandPalette();
    void CloseCommandPalette();
    IReadOnlyList<ICommandPaletteEntry> GetCommands();
    ICommandPaletteViewModel? CurrentModal { get; }
}