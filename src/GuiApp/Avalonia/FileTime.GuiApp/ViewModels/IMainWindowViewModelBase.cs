namespace FileTime.GuiApp.ViewModels;

public interface IMainWindowViewModelBase
{
    bool Loading { get; }
    IObservable<string?> MainFont { get; } 
}