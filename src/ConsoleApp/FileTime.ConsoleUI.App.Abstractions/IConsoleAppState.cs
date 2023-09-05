using System.Collections.ObjectModel;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Preview;

namespace FileTime.ConsoleUI.App;

public interface IConsoleAppState : IAppState
{
    ObservableCollection<string> PopupTexts { get; }
    ItemPreviewType? PreviewType { get; set; }
}