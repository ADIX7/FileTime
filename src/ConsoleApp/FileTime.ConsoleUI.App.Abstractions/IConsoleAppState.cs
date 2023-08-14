using System.Collections.ObjectModel;
using FileTime.App.Core.ViewModels;

namespace FileTime.ConsoleUI.App;

public interface IConsoleAppState : IAppState
{
    string ErrorText { get; set; }
    ObservableCollection<string> PopupTexts { get; }
}