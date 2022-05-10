using System.Collections.ObjectModel;
using FileTime.App.Core.Models.Enums;

namespace FileTime.App.Core.ViewModels;

public interface IAppState
{
    ObservableCollection<ITabViewModel> Tabs { get; }
    IObservable<ITabViewModel?> SelectedTab { get; }
    IObservable<string?> SearchText { get; }
    IObservable<ViewMode> ViewMode { get; }
    string RapidTravelText { get; set; }

    void AddTab(ITabViewModel tabViewModel);
    void RemoveTab(ITabViewModel tabViewModel);
    void SetSearchText(string? searchText);
    void SwitchViewMode(ViewMode newViewMode);
}