using Avalonia.Input;
using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.App.FrequencyNavigation.Services;
using FileTime.App.FuzzyPanel;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.FrequencyNavigation.ViewModels;

public class FrequencyNavigationViewModel : FuzzyPanelViewModel<string>, IFrequencyNavigationViewModel
{
    private readonly IFrequencyNavigationService _frequencyNavigationService;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly ITimelessContentProvider _timelessContentProvider;

    public FrequencyNavigationViewModel(
        IFrequencyNavigationService frequencyNavigationService,
        IUserCommandHandlerService userCommandHandlerService,
        ITimelessContentProvider timelessContentProvider)
    {
        _frequencyNavigationService = frequencyNavigationService;
        _userCommandHandlerService = userCommandHandlerService;
        _timelessContentProvider = timelessContentProvider;

        ShowWindow = _frequencyNavigationService.ShowWindow;
    }

    public void Close()
        => _frequencyNavigationService.CloseNavigationWindow();

    public async Task<bool> HandleKeyUp(KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Handled) return false;

        if (keyEventArgs.Key == Key.Enter)
        {
            keyEventArgs.Handled = true;
            var targetContainer = await _timelessContentProvider.GetItemByFullNameAsync(new FullName(SelectedItem), PointInTime.Present);
            var openContainerCommand = new OpenContainerCommand(new AbsolutePath(_timelessContentProvider, targetContainer));
            await _userCommandHandlerService.HandleCommandAsync(openContainerCommand);
            Close();
            return true;
        }

        return false;
    }

    public override async Task<bool> HandleKeyDown(KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Handled) return false;
        var handled = await base.HandleKeyDown(keyEventArgs);

        if (handled) return true;

        if (keyEventArgs.Key == Key.Escape)
        {
            keyEventArgs.Handled = true;
            Close();
            return true;
        }

        return false;
    }

    public override void UpdateFilteredMatches() =>
        FilteredMatches = new List<string>(_frequencyNavigationService.GetMatchingContainers(SearchText));

    string IModalViewModel.Name => "FrequencyNavigation";
}