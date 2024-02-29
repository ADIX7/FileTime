using System.Collections.ObjectModel;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Preview;
using FileTime.Core.Services;
using PropertyChanged.SourceGenerator;

namespace FileTime.ConsoleUI.App;

public partial class ConsoleAppState : AppStateBase, IConsoleAppState
{
    //TODO: make it thread safe
    public ObservableCollection<string> PopupTexts { get; } = new();
    
    [Notify] private ItemPreviewType? _previewType = ItemPreviewType.Binary;

    public ConsoleAppState(ITabEvents tabEvents) : base(tabEvents)
    {
    }
}