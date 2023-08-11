using DeclarativeProperty;
using FileTime.App.CommandPalette.Models;
using FileTime.App.CommandPalette.ViewModels;
using FileTime.App.Core.Services;
using PropertyChanged.SourceGenerator;

namespace FileTime.App.CommandPalette.Services;

public partial class CommandPaletteService : ICommandPaletteService
{
    private readonly IModalService _modalService;
    private readonly IIdentifiableUserCommandService _identifiableUserCommandService;
    private readonly DeclarativeProperty<bool> _showWindow = new(false);
    IDeclarativeProperty<bool> ICommandPaletteService.ShowWindow => _showWindow;
    [Notify] ICommandPaletteViewModel? _currentModal;

    public CommandPaletteService(
        IModalService modalService,
        IIdentifiableUserCommandService identifiableUserCommandService)
    {
        _modalService = modalService;
        _identifiableUserCommandService = identifiableUserCommandService;
    }
    public void OpenCommandPalette()
    {
        _showWindow.SetValueSafe(true);
        CurrentModal = _modalService.OpenModal<ICommandPaletteViewModel>();
    }

    public void CloseCommandPalette()
    {
        _showWindow.SetValueSafe(false);
        if (_currentModal is not null)
        {
            _modalService.CloseModal(_currentModal);
            CurrentModal = null;
        }
    }

    public IReadOnlyList<ICommandPaletteEntry> GetCommands() =>
        _identifiableUserCommandService
            .IdentifiableUserCommands
            .Select(c => new CommandPaletteEntry(c.Key, c.Value.Title))
            .OrderBy(c => c.Title)
            .ToList()
            .AsReadOnly();
}