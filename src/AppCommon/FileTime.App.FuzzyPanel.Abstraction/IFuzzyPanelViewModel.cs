using GeneralInputKey;

namespace FileTime.App.FuzzyPanel;

public interface IFuzzyPanelViewModel<TItem> where TItem : class
{
    List<TItem> FilteredMatches { get; }
    TItem? SelectedItem { get; }
    string SearchText { get; set; }
    Task UpdateFilteredMatches();
    Task<bool> HandleKeyDown(GeneralKeyEventArgs keyEventArgs);
}