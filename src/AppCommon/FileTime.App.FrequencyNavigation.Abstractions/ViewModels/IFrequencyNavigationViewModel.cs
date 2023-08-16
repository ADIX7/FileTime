using DeclarativeProperty;
using FileTime.App.Core.ViewModels;
using FileTime.App.FuzzyPanel;
using GeneralInputKey;

namespace FileTime.App.FrequencyNavigation.ViewModels;

public interface IFrequencyNavigationViewModel : IFuzzyPanelViewModel<string>, IModalViewModel
{
    IDeclarativeProperty<bool> ShowWindow { get; }
    void Close();
    Task<bool> HandleKeyUp(GeneralKeyEventArgs keyEventArgs);
}