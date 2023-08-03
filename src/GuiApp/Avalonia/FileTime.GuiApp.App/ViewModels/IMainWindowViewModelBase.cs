using DeclarativeProperty;

namespace FileTime.GuiApp.App.ViewModels;

public interface IMainWindowViewModelBase
{
    DeclarativeProperty<string> Title { get; }
    bool Loading { get; }
    IObservable<string?> MainFont { get; } 
    DeclarativeProperty<string> FatalError { get; }
}