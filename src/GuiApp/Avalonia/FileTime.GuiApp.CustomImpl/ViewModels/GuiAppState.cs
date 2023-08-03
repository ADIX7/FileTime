using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.Timeline;
using FileTime.Core.Models;
using FileTime.GuiApp.App.Configuration;
using FileTime.GuiApp.App.Models;
using FileTime.GuiApp.App.ViewModels;
using MvvmGen;

namespace FileTime.GuiApp.CustomImpl.ViewModels;

[ViewModel(GenerateConstructor = false)]
public partial class GuiAppState : AppStateBase, IGuiAppState, IDisposable
{
    private readonly BehaviorSubject<GuiPanel> _activePanel = new(GuiPanel.FileBrowser);

    public GuiAppState(ITimelineViewModel timelineViewModel) : base(timelineViewModel)
    {
        ActivePanel = _activePanel.AsObservable();
    }

    [Property] private bool _isAllShortcutVisible;

    [Property] private bool _noCommandFound;

    [Property] private List<CommandBindingConfiguration> _possibleCommands = new();

    [Property] private ObservableCollection<RootDriveInfo> _rootDriveInfos = new();

    [Property] private IReadOnlyList<PlaceInfo> _places = new List<PlaceInfo>();

    public List<KeyConfig> PreviousKeys { get; } = new();
    public ObservableCollection<string> PopupTexts { get; } = new();

    public IObservable<GuiPanel> ActivePanel { get; }

    public void SetActivePanel(GuiPanel newPanel)
        => _activePanel.OnNext(newPanel);

    public void Dispose()
    {
        _activePanel.Dispose();
    }
}
