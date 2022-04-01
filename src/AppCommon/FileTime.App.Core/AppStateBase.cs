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
        public ObservableCollection<ITabViewModel> Tabs { get; } = new();
        public IObservable<string?> SearchText { get; private set; }

        [Property]
        private ITabViewModel? _selectedTab;

        partial void OnInitialize()
        {
            SearchText = _searchText.AsObservable();
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
    }
}