using System.Reactive.Subjects;

namespace FileTime.GuiApp.ViewModels;

public class MainWindowLoadingViewModel : IMainWindowViewModelBase
{
    public bool Loading => true;
    public IObservable<string?> MainFont { get; } = new BehaviorSubject<string?>("");
}