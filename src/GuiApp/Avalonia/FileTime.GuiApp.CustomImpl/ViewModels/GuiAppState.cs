using System.Collections.ObjectModel;
using FileTime.App.Core.Models;
using FileTime.App.Core.ViewModels;
using FileTime.GuiApp.Configuration;
using MvvmGen;

namespace FileTime.GuiApp.ViewModels;

[ViewModel]
public partial class GuiAppState : AppStateBase, IGuiAppState
{
    [Property] private bool _isAllShortcutVisible;

    [Property] private bool _noCommandFound;

    [Property] private string? _messageBoxText;

    [Property] private List<CommandBindingConfiguration> _possibleCommands = new();

    [Property] private BindedCollection<RootDriveInfo> _rootDriveInfos = new();

    public List<KeyConfig> PreviousKeys { get; } = new();
}