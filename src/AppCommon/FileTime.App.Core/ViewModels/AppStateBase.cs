using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using FileTime.App.Core.Models.Enums;
using MvvmGen;
using MoreLinq;

namespace FileTime.App.Core.ViewModels;

[ViewModel]
public abstract partial class AppStateBase : IAppState
{
    private readonly BehaviorSubject<string?> _searchText = new(null);
    private readonly BehaviorSubject<ITabViewModel?> _selectedTab = new(null);
    private readonly BehaviorSubject<ViewMode> _viewMode = new(Models.Enums.ViewMode.Default);
    private readonly SourceList<ITabViewModel> _tabs = new();

    public IObservable<ViewMode> ViewMode { get; private set; }

    public ReadOnlyObservableCollection<ITabViewModel> Tabs { get; private set; }
    public IObservable<string?> SearchText { get; private set; }

    public IObservable<ITabViewModel?> SelectedTab { get; private set; }
    public ITabViewModel? CurrentSelectedTab { get; private set; }

    [Property] private string _rapidTravelText = "";

    partial void OnInitialize()
    {
        ViewMode = _viewMode.AsObservable();

        var tabsObservable = _tabs.Connect();

        SearchText = _searchText.AsObservable();
        SelectedTab = Observable.CombineLatest(tabsObservable.ToCollection(), _selectedTab.DistinctUntilChanged(), GetSelectedTab);

        SelectedTab.Subscribe(t =>
        {
            _selectedTab.OnNext(t);
            CurrentSelectedTab = t;
        });

        tabsObservable
            .Bind(out var collection)
            .DisposeMany()
            .Subscribe();

        Tabs = collection;
    }

    public void AddTab(ITabViewModel tabViewModel)
    {
        if (_tabs.Items.Any(t => t.TabNumber == tabViewModel.TabNumber))
            throw new ArgumentException($"There is a tab with the same tab number {tabViewModel.TabNumber}.", nameof(tabViewModel));

        var index = _tabs.Items.Count(t => t.TabNumber < tabViewModel.TabNumber);
        _tabs.Insert(index, tabViewModel);
    }

    public void RemoveTab(ITabViewModel tabViewModel)
    {
        if (!_tabs.Items.Contains(tabViewModel)) return;

        _tabs.Remove(tabViewModel);
    }

    public void SetSearchText(string? searchText) => _searchText.OnNext(searchText);

    public void SwitchViewMode(ViewMode newViewMode)
    {
        _viewMode.OnNext(newViewMode);
    }

    public void SetSelectedTab(ITabViewModel tabToSelect) => _selectedTab.OnNext(tabToSelect);

    private ITabViewModel? GetSelectedTab(IEnumerable<ITabViewModel> tabs, ITabViewModel? expectedSelectedTab)
    {
        var (preferred, others) = tabs.OrderBy(t => t.TabNumber).Partition(t => t.TabNumber >= (expectedSelectedTab?.TabNumber ?? 0));
        return preferred.Concat(others.Reverse()).FirstOrDefault();
    }
}