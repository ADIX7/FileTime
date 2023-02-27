using Avalonia.Input;
using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.App.FrequencyNavigation.Services;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using MvvmGen;

namespace FileTime.App.FrequencyNavigation.ViewModels;

[ViewModel]
[Inject(typeof(IFrequencyNavigationService), "_frequencyNavigationService")]
[Inject(typeof(IUserCommandHandlerService), "_userCommandHandlerService")]
[Inject(typeof(ITimelessContentProvider), "_timelessContentProvider")]
public partial class FrequencyNavigationViewModel : IFrequencyNavigationViewModel
{
    private string _searchText;

    [Property] private IObservable<bool> _showWindow;
    [Property] private List<string> _filteredMatches;
    [Property] private string _selectedItem;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;

            _searchText = value;
            OnPropertyChanged();

            UpdateFilteredMatches();
        }
    }

    public void Close()
        => _frequencyNavigationService.CloseNavigationWindow();

    public async void HandleKeyDown(KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Key == Key.Down)
        {
            var nextItem = FilteredMatches.SkipWhile(i => i != SelectedItem).Skip(1).FirstOrDefault();

            if (nextItem is not null)
            {
                SelectedItem = nextItem;
            }
        }
        else if (keyEventArgs.Key == Key.Up)
        {
            var previousItem = FilteredMatches.TakeWhile(i => i != SelectedItem).LastOrDefault();

            if (previousItem is not null)
            {
                SelectedItem = previousItem;
            }
        }
        else if (keyEventArgs.Key == Key.Enter)
        {
            var targetContainer = await _timelessContentProvider.GetItemByFullNameAsync(new FullName(SelectedItem), PointInTime.Present);
            var openContainerCommand = new OpenContainerCommand(new AbsolutePath(_timelessContentProvider, targetContainer));
            await _userCommandHandlerService.HandleCommandAsync(openContainerCommand);
            Close();
        }
    }

    partial void OnInitialize()
        => _showWindow = _frequencyNavigationService.ShowWindow;

    private void UpdateFilteredMatches()
    {
        FilteredMatches = new List<string>(_frequencyNavigationService.GetMatchingContainers(_searchText));
        if (FilteredMatches.Contains(SelectedItem)) return;

        SelectedItem = FilteredMatches.Count > 0
            ? FilteredMatches[0]
            : null;
    }

    string IModalViewModel.Name => "FrequencyNavigation";
}