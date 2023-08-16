using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Input;
using FileTime.App.Core.Configuration;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.Timeline;
using FileTime.GuiApp.App.Models;
using FileTime.GuiApp.App.ViewModels;
using FileTime.Providers.Local;
using MvvmGen;
using PropertyChanged.SourceGenerator;

namespace FileTime.GuiApp.CustomImpl.ViewModels;

public partial class GuiAppState : AppStateBase, IGuiAppState, IDisposable
{
    private readonly BehaviorSubject<GuiPanel> _activePanel = new(GuiPanel.FileBrowser);

    [Notify] private ObservableCollection<RootDriveInfo> _rootDriveInfos;

    [Notify] private IReadOnlyList<PlaceInfo> _places = new List<PlaceInfo>();
    public ObservableCollection<string> PopupTexts { get; } = new();

    public IObservable<GuiPanel> ActivePanel { get; }

    public GuiAppState(IRootDriveInfoService rootDriveInfoService)
    {
        ActivePanel = _activePanel.AsObservable();
        _rootDriveInfos = rootDriveInfoService.RootDriveInfos;
    }

    public void SetActivePanel(GuiPanel newPanel)
        => _activePanel.OnNext(newPanel);

    public void Dispose() => _activePanel.Dispose();
}
