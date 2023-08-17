using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.App.FuzzyPanel;
using GeneralInputKey;
using Microsoft.Extensions.Logging;

namespace FileTime.App.CommandPalette.ViewModels;

public class CommandPaletteViewModel : FuzzyPanelViewModel<ICommandPaletteEntryViewModel>, ICommandPaletteViewModel
{
    private readonly ICommandPaletteService _commandPaletteService;
    private readonly IIdentifiableUserCommandService _identifiableUserCommandService;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly ICommandKeysHelperService _commandKeysHelperService;
    private readonly ILogger<CommandPaletteViewModel> _logger;
    string IModalViewModel.Name => "CommandPalette";

    public CommandPaletteViewModel(
        ICommandPaletteService commandPaletteService,
        IIdentifiableUserCommandService identifiableUserCommandService,
        IUserCommandHandlerService userCommandHandlerService,
        ICommandKeysHelperService commandKeysHelperService,
        ILogger<CommandPaletteViewModel> logger)
        : base((a, b) => a.Identifier == b.Identifier)
    {
        _commandPaletteService = commandPaletteService;
        _identifiableUserCommandService = identifiableUserCommandService;
        _userCommandHandlerService = userCommandHandlerService;
        _commandKeysHelperService = commandKeysHelperService;
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
            {
                var searchTerms = SearchText.Split(' ');
                return searchTerms
                    .All(s =>
                        c.Title.Contains(s, StringComparison.OrdinalIgnoreCase)
                        || c.Identifier.Contains(s, StringComparison.OrdinalIgnoreCase)
                    );
            })
            .Select(c =>
                (ICommandPaletteEntryViewModel) new CommandPaletteEntryViewModel(c.Identifier, c.Title, _commandKeysHelperService.GetKeyConfigsString(c.Identifier))
            )
            .ToList();

    public override async Task<bool> HandleKeyDown(GeneralKeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Handled) return false;

        var handled = await base.HandleKeyDown(keyEventArgs);
        if (handled)
        {
            return true;
        }

        if (keyEventArgs.Key == Keys.Escape)
        {
            keyEventArgs.Handled = true;
            Close();
            return true;
        }

        return false;
    }

    public async Task<bool> HandleKeyUp(GeneralKeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Handled) return false;

        if (keyEventArgs.Key == Keys.Enter)
        {
            if (SelectedItem is null) return false;

            var command = _identifiableUserCommandService.GetCommand(SelectedItem.Identifier);
            if (command is null) return false;

            keyEventArgs.Handled = true;
            Close();
            SearchText = string.Empty;

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