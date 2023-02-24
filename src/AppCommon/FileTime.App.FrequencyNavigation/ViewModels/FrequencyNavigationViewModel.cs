using FileTime.App.Core.ViewModels;
using FileTime.App.FrequencyNavigation.Services;
using MvvmGen;

namespace FileTime.App.FrequencyNavigation.ViewModels;

[ViewModel]
[Inject(typeof(IFrequencyNavigationService), "_frequencyNavigationService")]
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
    {
        _frequencyNavigationService.CloseNavigationWindow();
    }

    partial void OnInitialize()
    {
        _showWindow = _frequencyNavigationService.ShowWindow;
    }

    private void UpdateFilteredMatches()
    {
        FilteredMatches = new List<string>(_frequencyNavigationService.GetMatchingContainers(_searchText));
    }

    string IModalViewModel.Name => "FrequencyNavigation";
}