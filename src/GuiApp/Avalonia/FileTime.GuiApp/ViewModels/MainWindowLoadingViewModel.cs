using System.Reactive.Subjects;
using DeclarativeProperty;

namespace FileTime.GuiApp.ViewModels;

public class MainWindowLoadingViewModel : IMainWindowViewModelBase
{
    public bool Loading => true;
    public IObservable<string?> MainFont { get; } = new BehaviorSubject<string?>("");
    public DeclarativeProperty<string> Title { get; } = new("Loading...");
    public DeclarativeProperty<string> FatalError { get; } = new();
}