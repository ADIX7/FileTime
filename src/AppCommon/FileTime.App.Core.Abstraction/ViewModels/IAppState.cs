using System.Collections.ObjectModel;
using FileTime.App.Core.Models.Enums;

namespace FileTime.App.Core.ViewModels;

public interface IAppState
{
    ReadOnlyObservableCollection<ITabViewModel> Tabs { get; }
    IObservable<ITabViewModel?> SelectedTab { get; }
    IObservable<string?> SearchText { get; }
    IObservable<ViewMode> ViewMode { get; }
    string RapidTravelText { get; set; }
    ITabViewModel? CurrentSelectedTab { get; }

    void AddTab(ITabViewModel tabViewModel);
    void RemoveTab(ITabViewModel tabViewModel);
    void SetSearchText(string? searchText);
    void SwitchViewMode(ViewMode newViewMode);
    void SetSelectedTab(ITabViewModel tabToSelect);
}