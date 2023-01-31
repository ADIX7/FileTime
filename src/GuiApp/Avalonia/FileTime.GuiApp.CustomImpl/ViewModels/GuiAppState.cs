using System.Collections.ObjectModel;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.Timeline;
using FileTime.Core.Models;
using FileTime.GuiApp.Configuration;
using FileTime.GuiApp.ViewModels;
using MvvmGen;

namespace FileTime.GuiApp.CustomImpl.ViewModels;

[ViewModel(GenerateConstructor = false)]
public partial class GuiAppState : AppStateBase, IGuiAppState
{
    public GuiAppState(ITimelineViewModel timelineViewModel) : base(timelineViewModel)
    {
    }

    [Property] private bool _isAllShortcutVisible;

    [Property] private bool _noCommandFound;

    [Property] private List<CommandBindingConfiguration> _possibleCommands = new();

    [Property] private BindedCollection<RootDriveInfo, string> _rootDriveInfos = new();

    [Property] private IReadOnlyList<PlaceInfo> _places = new List<PlaceInfo>();

    public List<KeyConfig> PreviousKeys { get; } = new();
    public ObservableCollection<string> PopupTexts { get; } = new();
}