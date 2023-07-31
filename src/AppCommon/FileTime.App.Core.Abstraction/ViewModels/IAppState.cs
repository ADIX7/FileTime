using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels.Timeline;

namespace FileTime.App.Core.ViewModels;

public interface IAppState
{
    ReadOnlyObservableCollection<ITabViewModel> Tabs { get; }
    IObservable<ITabViewModel?> SelectedTab { get; }
    IObservable<string?> SearchText { get; }
    IDeclarativeProperty<ViewMode> ViewMode { get; }
    DeclarativeProperty<string?> RapidTravelText { get; }
    ITabViewModel? CurrentSelectedTab { get; }
    ITimelineViewModel TimelineViewModel { get; }

    void AddTab(ITabViewModel tabViewModel);
    void RemoveTab(ITabViewModel tabViewModel);
    void SetSearchText(string? searchText);
    Task SwitchViewModeAsync(ViewMode newViewMode);
    void SetSelectedTab(ITabViewModel tabToSelect);
}