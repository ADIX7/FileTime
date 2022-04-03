using System.Collections.ObjectModel;
using FileTime.App.Core.ViewModels;

namespace FileTime.App.Core
{
    public interface IAppState
    {
        ObservableCollection<ITabViewModel> Tabs { get; }
        ITabViewModel? SelectedTab { get; }
        IObservable<ITabViewModel?> SelectedTabObservable { get; }
        IObservable<string?> SearchText { get; }

        void AddTab(ITabViewModel tabViewModel);
        void RemoveTab(ITabViewModel tabViewModel);
        void SetSearchText(string? searchText);
    }
}