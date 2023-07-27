using Avalonia.Input;
using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.App.FuzzyPanel;
using Microsoft.Extensions.Logging;

namespace FileTime.App.CommandPalette.ViewModels;

public class CommandPaletteViewModel : FuzzyPanelViewModel<ICommandPaletteEntryViewModel>, ICommandPaletteViewModel
{
    private readonly ICommandPaletteService _commandPaletteService;
    private readonly IIdentifiableUserCommandService _identifiableUserCommandService;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly ILogger<CommandPaletteViewModel> _logger;
    string IModalViewModel.Name => "CommandPalette";

    public CommandPaletteViewModel(
        ICommandPaletteService commandPaletteService,
        IIdentifiableUserCommandService identifiableUserCommandService,
        IUserCommandHandlerService userCommandHandlerService,
        ILogger<CommandPaletteViewModel> logger)
    {
        _commandPaletteService = commandPaletteService;
        _identifiableUserCommandService = identifiableUserCommandService;
        _userCommandHandlerService = userCommandHandlerService;
        _logger = logger;
        ShowWindow = _commandPaletteService.ShowWindow;
        UpdateFilteredMatchesInternal();
    }

    public void Close() => _commandPaletteService.CloseCommandPalette();

    public override void UpdateFilteredMatches() => UpdateFilteredMatchesInternal();

    private void UpdateFilteredMatchesInternal() =>
        FilteredMatches = _commandPaletteService
            .GetCommands()
            .Where(c =>
                c.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || c.Identifier.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            )
            .Select(c =>
                (ICommandPaletteEntryViewModel) new CommandPaletteEntryViewModel(c.Identifier, c.Title))
            .Take(30) // TODO remove magic number
            .OrderBy(c => c.Title)
            .ToList();

    public override async Task<bool> HandleKeyDown(KeyEventArgs keyEventArgs)
    {
        var handled = await base.HandleKeyDown(keyEventArgs);
        if (handled)
        {
            return true;
        }

        if (keyEventArgs.Key == Key.Escape)
        {
            keyEventArgs.Handled = true;
            Close();
            return true;
        }

        if (keyEventArgs.Key == Key.Enter)
        {
            if (SelectedItem is null) return false;

            var command = _identifiableUserCommandService.GetCommand(SelectedItem.Identifier);
            if (command is null) return false;

            keyEventArgs.Handled = true;
            Close();

            try
            {
                await _userCommandHandlerService.HandleCommandAsync(command);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error while running command. {Command} {Error}", command.GetType().Name, e);
            }

            return true;
        }

        return false;
    }
}