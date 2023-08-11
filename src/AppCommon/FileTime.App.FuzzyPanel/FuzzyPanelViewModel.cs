using System.ComponentModel;
using DeclarativeProperty;
using FileTime.App.Core.Models;
using GeneralInputKey;
using PropertyChanged.SourceGenerator;

namespace FileTime.App.FuzzyPanel;

public abstract partial class FuzzyPanelViewModel<TItem> : IFuzzyPanelViewModel<TItem> where TItem : class
{
    private readonly Func<TItem, TItem, bool> _itemEquality;
    private string _searchText = String.Empty;

    [Notify(set: Setter.Protected)] private IDeclarativeProperty<bool> _showWindow;
    [Notify(set: Setter.Protected)] private List<TItem> _filteredMatches;
    [Notify(set: Setter.Protected)] private TItem? _selectedItem;

    protected FuzzyPanelViewModel(Func<TItem, TItem, bool>? itemEquality = null)
    {
        _itemEquality = itemEquality ?? ((a, b) => a == b);
    }

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

    public virtual Task<bool> HandleKeyDown(GeneralKeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Key == Keys.Down)
        {
            var nextItem = SelectedItem is null
                ? FilteredMatches.FirstOrDefault()
                : FilteredMatches.SkipWhile(i => !_itemEquality(i, SelectedItem)).Skip(1).FirstOrDefault();

            if (nextItem is not null)
            {
                keyEventArgs.Handled = true;
                SelectedItem = nextItem;
            }

            return Task.FromResult(true);
        }
        else if (keyEventArgs.Key == Keys.Up)
        {
            var previousItem = SelectedItem is null
                ? FilteredMatches.LastOrDefault()
                : FilteredMatches.TakeWhile(i => !_itemEquality(i, SelectedItem)).LastOrDefault();

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