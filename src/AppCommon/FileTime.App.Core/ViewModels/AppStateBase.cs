using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DeclarativeProperty;
using FileTime.App.Core.Configuration;
using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models.Extensions;
using MoreLinq;
using PropertyChanged.SourceGenerator;

namespace FileTime.App.Core.ViewModels;

public abstract partial class AppStateBase : IAppState
{
    private readonly BehaviorSubject<string?> _searchText = new(null);
    private readonly DeclarativeProperty<ITabViewModel?> _selectedTab = new();
    private readonly DeclarativeProperty<ViewMode> _viewMode = new(Models.Enums.ViewMode.Default);
    private readonly ObservableCollection<ITabViewModel> _tabs = new();
    private readonly DeclarativeProperty<string?> _rapidTravelText;

    public IDeclarativeProperty<ViewMode> ViewMode { get; }

    public ReadOnlyObservableCollection<ITabViewModel> Tabs { get; }
    public IObservable<string?> SearchText { get; }

    public IDeclarativeProperty<ITabViewModel?> SelectedTab { get; }
    public IDeclarativeProperty<string?> RapidTravelText { get; }
    public IDeclarativeProperty<string?> RapidTravelTextDebounced { get; }

    public IDeclarativeProperty<string?> ContainerStatus { get; }
    [Notify] public List<KeyConfig> PreviousKeys { get; } = new();
    [Notify] public bool NoCommandFound { get; set; }

    protected AppStateBase()
    {
        _rapidTravelText = new ("");
        RapidTravelText = _rapidTravelText.DistinctUntilChanged();
        RapidTravelTextDebounced = RapidTravelText
            .Debounce(v =>
                    string.IsNullOrEmpty(v)
                        ? TimeSpan.Zero
                        : TimeSpan.FromMilliseconds(200)
                , resetTimer: true
            );

        ViewMode = _viewMode;

        SearchText = _searchText.AsObservable();
        SelectedTab = DeclarativePropertyHelpers.CombineLatest<ObservableCollection<ITabViewModel>, ITabViewModel, ITabViewModel>(
            _tabs.Watch()!,
            _selectedTab!,
            (tabs, selectedTab) => Task.FromResult(GetSelectedTab(tabs, selectedTab)!)
        )!;

        Tabs = new ReadOnlyObservableCollection<ITabViewModel>(_tabs);

        ContainerStatus = SelectedTab
            .Map(t => t?.CurrentLocation)
            .Switch()
            .Map(c => c?.GetExtension<StatusProviderContainerExtension>()?.GetStatusProperty())
            .Switch()!;
    }

    public void AddTab(ITabViewModel tabViewModel)
    {
        if (_tabs.Any(t => t.TabNumber == tabViewModel.TabNumber))
            throw new ArgumentException($"There is a tab with the same tab number {tabViewModel.TabNumber}.", nameof(tabViewModel));

        var index = _tabs.Count(t => t.TabNumber < tabViewModel.TabNumber);
        _tabs.Insert(index, tabViewModel);
    }

    public void RemoveTab(ITabViewModel tabViewModel)
    {
        if (!_tabs.Contains(tabViewModel)) return;

        _tabs.Remove(tabViewModel);
    }

    public void SetSearchText(string? searchText) => _searchText.OnNext(searchText);

    public async Task SwitchViewModeAsync(ViewMode newViewMode) => await _viewMode.SetValue(newViewMode);

    public async Task SetSelectedTabAsync(ITabViewModel tabToSelect) => await _selectedTab.SetValue(tabToSelect);
    public async Task SetRapidTravelTextAsync(string? text) => await _rapidTravelText.SetValue(text);

    private ITabViewModel? GetSelectedTab(IEnumerable<ITabViewModel> tabs, ITabViewModel? expectedSelectedTab)
    {
        var (preferred, others) =
            tabs
                .OrderBy(t => t.TabNumber)
                .Partition(t => t.TabNumber >= (expectedSelectedTab?.TabNumber ?? 0));
        return preferred.Concat(others.Reverse()).FirstOrDefault();
    }
}