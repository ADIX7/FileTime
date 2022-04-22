using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.App.Core.Models.Enums;
using MvvmGen;
using MoreLinq;

namespace FileTime.App.Core.ViewModels
{
    [ViewModel]
    public abstract partial class AppStateBase : IAppState
    {
        private readonly BehaviorSubject<string?> _searchText = new(null);
        private readonly BehaviorSubject<ITabViewModel?> _selectedTab = new(null);
        private readonly BehaviorSubject<IEnumerable<ITabViewModel>> _tabs = new(Enumerable.Empty<ITabViewModel>());

        [Property]
        private ViewMode _viewMode;

        public ObservableCollection<ITabViewModel> Tabs { get; } = new();
        public IObservable<string?> SearchText { get; private set; }

        public IObservable<ITabViewModel?> SelectedTab { get; private set; }

        partial void OnInitialize()
        {
            Tabs.CollectionChanged += (_, _) => _tabs.OnNext(Tabs);
            SearchText = _searchText.AsObservable();
            SelectedTab = Observable.CombineLatest(_tabs, _selectedTab, GetSelectedTab);
        }

        public void AddTab(ITabViewModel tabViewModel)
        {
            Tabs.Add(tabViewModel);
        }

        public void RemoveTab(ITabViewModel tabViewModel)
        {
            if (!Tabs.Contains(tabViewModel)) return;

            Tabs.Remove(tabViewModel);
        }

        public void SetSearchText(string? searchText) => _searchText.OnNext(searchText);

        public void SetSelectedTab(ITabViewModel tabToSelect) => _selectedTab.OnNext(tabToSelect);

        private ITabViewModel? GetSelectedTab(IEnumerable<ITabViewModel> tabs, ITabViewModel? expectedSelectedTab)
        {
            var (prefered, others) = tabs.OrderBy(t => t.TabNumber).Partition(t => t.TabNumber >= (expectedSelectedTab?.TabNumber ?? 0));
            return prefered.Concat(others).FirstOrDefault();
        }
    }
}