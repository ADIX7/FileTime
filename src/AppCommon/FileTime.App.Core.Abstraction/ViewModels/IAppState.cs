using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.App.Core.Configuration;
using FileTime.App.Core.Models.Enums;

namespace FileTime.App.Core.ViewModels;

public interface IAppState
{
    ReadOnlyObservableCollection<ITabViewModel> Tabs { get; }
    IDeclarativeProperty<ITabViewModel?> SelectedTab { get; }
    IObservable<string?> SearchText { get; }
    IDeclarativeProperty<ViewMode> ViewMode { get; }
    IDeclarativeProperty<string?> RapidTravelText { get; }
    IDeclarativeProperty<string?> RapidTravelTextDebounced { get; }
    IDeclarativeProperty<string?> ContainerStatus { get; }
    List<KeyConfig> PreviousKeys { get; }
    bool NoCommandFound { get; set; }

    void AddTab(ITabViewModel tabViewModel);
    void RemoveTab(ITabViewModel tabViewModel);
    void SetSearchText(string? searchText);
    Task SwitchViewModeAsync(ViewMode newViewMode);
    Task SetSelectedTabAsync(ITabViewModel tabToSelect);
    Task SetRapidTravelTextAsync(string? text);
}