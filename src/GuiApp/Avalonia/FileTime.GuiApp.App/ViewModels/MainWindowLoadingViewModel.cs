using System.Reactive.Subjects;
using Avalonia.Controls;
using DeclarativeProperty;

namespace FileTime.GuiApp.App.ViewModels;

public class MainWindowLoadingViewModel : IMainWindowViewModelBase
{
    public bool Loading => true;
    public IObservable<string?> MainFont { get; } = new BehaviorSubject<string?>("");
    public DeclarativeProperty<string> Title { get; } = new("Loading...");
    public DeclarativeProperty<string?> FatalError { get; } = new(null);
    public IReadOnlyList<WindowTransparencyLevel> TransparencyLevelHint { get; } = new[] {WindowTransparencyLevel.AcrylicBlur};
}