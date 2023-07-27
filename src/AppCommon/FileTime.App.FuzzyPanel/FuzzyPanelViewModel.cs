using System.ComponentModel;
using Avalonia.Input;
using PropertyChanged.SourceGenerator;

namespace FileTime.App.FuzzyPanel;

public abstract partial class FuzzyPanelViewModel<TItem> : IFuzzyPanelViewModel<TItem> where TItem : class
{
    private string _searchText = String.Empty;

    [Notify(set: Setter.Protected)] private IObservable<bool> _showWindow;
    [Notify(set: Setter.Protected)] private List<TItem> _filteredMatches;
    [Notify(set: Setter.Protected)] private TItem? _selectedItem;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;

            _searchText = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SearchText)));

            UpdateFilteredMatchesInternal();
        }
    }

    private void UpdateFilteredMatchesInternal()
    {
        UpdateFilteredMatches();
        if (SelectedItem != null && FilteredMatches.Contains(SelectedItem)) return;

        SelectedItem = FilteredMatches.Count > 0
            ? FilteredMatches[0]
            : null;
    }

    public abstract void UpdateFilteredMatches();

    public virtual Task<bool> HandleKeyDown(KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Key == Key.Down)
        {
            var nextItem = FilteredMatches.SkipWhile(i => i != SelectedItem).Skip(1).FirstOrDefault();

            if (nextItem is not null)
            {
                keyEventArgs.Handled = true;
                SelectedItem = nextItem;
            }

            return Task.FromResult(true);
        }
        else if (keyEventArgs.Key == Key.Up)
        {
            var previousItem = FilteredMatches.TakeWhile(i => i != SelectedItem).LastOrDefault();

            if (previousItem is not null)
            {
                keyEventArgs.Handled = true;
                SelectedItem = previousItem;
            }

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}