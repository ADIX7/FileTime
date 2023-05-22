using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.App.CommandPalette.Models;
using FileTime.App.CommandPalette.ViewModels;
using FileTime.App.Core.Services;
using PropertyChanged.SourceGenerator;

namespace FileTime.App.CommandPalette.Services;

public partial class CommandPaletteService : ICommandPaletteService
{
    private readonly IModalService _modalService;
    private readonly IIdentifiableUserCommandService _identifiableUserCommandService;
    private readonly BehaviorSubject<bool> _showWindow = new(false);
    IObservable<bool> ICommandPaletteService.ShowWindow => _showWindow.AsObservable();
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
        _showWindow.OnNext(true);
        CurrentModal = _modalService.OpenModal<ICommandPaletteViewModel>();
    }

    public IReadOnlyList<ICommandPaletteEntry> GetCommands() =>
        _identifiableUserCommandService
            .GetCommandIdentifiers()
            .Select(c => new CommandPaletteEntry(c, c))
            .ToList()
            .AsReadOnly();
}