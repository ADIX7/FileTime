using FileTime.App.Core.ViewModels;
using FileTime.GuiApp.Configuration;
using MvvmGen;

namespace FileTime.GuiApp.ViewModels;

[ViewModel]
public partial class GuiAppState : AppStateBase, IGuiAppState
{
    [Property]
    private bool _isAllShortcutVisible;

    [Property]
    private bool _noCommandFound;

    [Property]
    private string? _messageBoxText;

    [Property]
    private List<CommandBindingConfiguration> _possibleCommands = new();

    public List<KeyConfig> PreviousKeys { get; } = new();
}