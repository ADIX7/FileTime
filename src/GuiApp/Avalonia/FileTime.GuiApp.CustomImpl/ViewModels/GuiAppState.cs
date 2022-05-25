using FileTime.App.Core.Models;
using FileTime.App.Core.ViewModels;
using FileTime.GuiApp.Configuration;
using FileTime.GuiApp.ViewModels;
using MvvmGen;

namespace FileTime.GuiApp.CustomImpl.ViewModels;

[ViewModel]
public partial class GuiAppState : AppStateBase, IGuiAppState
{
    [Property] private bool _isAllShortcutVisible;

    [Property] private bool _noCommandFound;

    [Property] private string? _messageBoxText;

    [Property] private List<CommandBindingConfiguration> _possibleCommands = new();

    [Property] private BindedCollection<RootDriveInfo, string> _rootDriveInfos = new();

    public List<KeyConfig> PreviousKeys { get; } = new();
}