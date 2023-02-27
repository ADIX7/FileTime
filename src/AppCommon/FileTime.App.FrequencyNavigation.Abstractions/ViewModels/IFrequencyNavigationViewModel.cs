using Avalonia.Input;
using FileTime.App.Core.ViewModels;

namespace FileTime.App.FrequencyNavigation.ViewModels;

public interface IFrequencyNavigationViewModel : IModalViewModel
{
    IObservable<bool> ShowWindow { get; }
    List<string> FilteredMatches { get; }
    string SearchText { get; set; }
    string SelectedItem { get; set; }
    void Close();
    void HandleKeyDown(KeyEventArgs keyEventArgs);
}