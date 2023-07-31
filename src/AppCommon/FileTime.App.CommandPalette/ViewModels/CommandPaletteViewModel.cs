using System.Text;
using Avalonia.Input;
using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.App.FuzzyPanel;
using FileTime.GuiApp.Configuration;
using FileTime.GuiApp.Services;
using Microsoft.Extensions.Logging;

namespace FileTime.App.CommandPalette.ViewModels;

public class CommandPaletteViewModel : FuzzyPanelViewModel<ICommandPaletteEntryViewModel>, ICommandPaletteViewModel
{
    private readonly ICommandPaletteService _commandPaletteService;
    private readonly IIdentifiableUserCommandService _identifiableUserCommandService;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly IKeyboardConfigurationService _keyboardConfigurationService;
    private readonly ILogger<CommandPaletteViewModel> _logger;
    string IModalViewModel.Name => "CommandPalette";

    public CommandPaletteViewModel(
        ICommandPaletteService commandPaletteService,
        IIdentifiableUserCommandService identifiableUserCommandService,
        IUserCommandHandlerService userCommandHandlerService,
        IKeyboardConfigurationService keyboardConfigurationService,
        ILogger<CommandPaletteViewModel> logger)
        : base((a, b) => a.Identifier == b.Identifier)
    {
        _commandPaletteService = commandPaletteService;
        _identifiableUserCommandService = identifiableUserCommandService;
        _userCommandHandlerService = userCommandHandlerService;
        _keyboardConfigurationService = keyboardConfigurationService;
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
                (ICommandPaletteEntryViewModel) new CommandPaletteEntryViewModel(c.Identifier, c.Title, GetKeyConfigsString(c.Identifier))
            )
            .ToList();

    private string GetKeyConfigsString(string commandIdentifier)
    {
        var keyConfigs = GetKeyConfigsForCommand(commandIdentifier);
        if (keyConfigs.Count == 0) return string.Empty;

        return string.Join(
            " ; ",
            keyConfigs
                .Select(ks =>
                    string.Join(
                        ", ",
                        ks.Select(FormatKeyConfig)
                    )
                )
        );
    }

    private string FormatKeyConfig(KeyConfig keyConfig)
    {
        var stringBuilder = new StringBuilder();

        if (keyConfig.Ctrl) stringBuilder.Append("Ctrl + ");
        if (keyConfig.Shift) stringBuilder.Append("Shift + ");
        if (keyConfig.Alt) stringBuilder.Append("Alt + ");

        stringBuilder.Append(keyConfig.Key.ToString());

        return stringBuilder.ToString();
    }

    private List<List<KeyConfig>> GetKeyConfigsForCommand(string commandIdentifier)
        => _keyboardConfigurationService
            .AllShortcut
            .Where(s => s.Command == commandIdentifier)
            .Select(k => k.Keys)
            .ToList();

    public override async Task<bool> HandleKeyDown(KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Handled) return false;

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

        return false;
    }

    public async Task<bool> HandleKeyUp(KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Handled) return false;

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