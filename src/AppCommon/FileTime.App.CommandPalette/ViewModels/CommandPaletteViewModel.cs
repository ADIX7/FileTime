using Avalonia.Input;
using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.ViewModels;
using MvvmGen;

namespace FileTime.App.CommandPalette.ViewModels;

[ViewModel]
[Inject(typeof(ICommandPaletteService), "_commandPaletteService")]
public partial class CommandPaletteViewModel : ICommandPaletteViewModel
{
    private string _searchText;

    [Property] private IObservable<bool> _showWindow;
    [Property] private List<ICommandPaletteEntryViewModel> _filteredMatches;
    [Property] private ICommandPaletteEntryViewModel? _selectedItem;
    string IModalViewModel.Name => "CommandPalette";

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

    public void Close() => throw new NotImplementedException();

    public void HandleKeyDown(KeyEventArgs keyEventArgs) => throw new NotImplementedException();

    partial void OnInitialize()  
    {
        ShowWindow = _commandPaletteService.ShowWindow;
        UpdateFilteredMatches();
    }

    private void UpdateFilteredMatches()
    {
        FilteredMatches = _commandPaletteService
            .GetCommands()
            .Select(c =>
                (ICommandPaletteEntryViewModel) new CommandPaletteEntryViewModel(c.Identifier, c.Title))
            .Take(30) // TODO remove magic number
            .OrderBy(c => c.Title)
            .ToList();

        if (SelectedItem != null && FilteredMatches.Contains(SelectedItem)) return;

        SelectedItem = FilteredMatches.Count > 0
            ? FilteredMatches[0]
            : null;
    }
}