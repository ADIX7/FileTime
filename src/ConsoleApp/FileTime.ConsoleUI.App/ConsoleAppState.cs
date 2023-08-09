using FileTime.App.Core.ViewModels;
using PropertyChanged.SourceGenerator;

namespace FileTime.ConsoleUI.App;

public partial class ConsoleAppState : AppStateBase, IConsoleAppState
{
    [Notify] private string? _errorText;
}