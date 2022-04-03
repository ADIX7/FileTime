using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.App.Core.ViewModels;
using MvvmGen;

namespace FileTime.App.Core
{
    [ViewModel]
    public abstract partial class AppStateBase : IAppState
    {
        private readonly BehaviorSubject<string?> _searchText = new(null);
        private readonly BehaviorSubject<ITabViewModel?> _selectedTabObservable = new(null);
        private ITabViewModel? _selectedTab;

        public ObservableCollection<ITabViewModel> Tabs { get; } = new();
        public IObservable<string?> SearchText { get; private set; }

        public IObservable<ITabViewModel?> SelectedTabObservable { get; private set; }
        public ITabViewModel? SelectedTab
        {
            get => _selectedTab;
            private set
            {
                if (value != _selectedTab)
                {
                    _selectedTab = value;
                    OnPropertyChanged(nameof(SelectedTab));
                    _selectedTabObservable.OnNext(value);
                }
            }
        }

        partial void OnInitialize()
        {
            SearchText = _searchText.AsObservable();
            SelectedTabObservable = _selectedTabObservable.AsObservable();
        }

        public void AddTab(ITabViewModel tabViewModel)
        {
            Tabs.Add(tabViewModel);
            if (_selectedTab == null)
            {
                SelectedTab = Tabs.First();
            }
        }

        public void RemoveTab(ITabViewModel tabViewModel)
        {
            if (!Tabs.Contains(tabViewModel)) return;

            Tabs.Remove(tabViewModel);
            if (_selectedTab == tabViewModel)
            {
                SelectedTab = Tabs.FirstOrDefault();
            }
        }

        public void SetSearchText(string? searchText)
        {
            _searchText.OnNext(searchText);
        }

        public void SetSelectedTab(ITabViewModel tabToSelect)
        {
            SelectedTab = tabToSelect;
        }
    }
}