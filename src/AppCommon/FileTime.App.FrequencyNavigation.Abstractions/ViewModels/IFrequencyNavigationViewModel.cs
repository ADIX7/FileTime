using FileTime.App.Core.Models;
using FileTime.App.Core.ViewModels;
using FileTime.App.FuzzyPanel;

namespace FileTime.App.FrequencyNavigation.ViewModels;

public interface IFrequencyNavigationViewModel : IFuzzyPanelViewModel<string>, IModalViewModel
{
    IObservable<bool> ShowWindow { get; }
    void Close();
    Task<bool> HandleKeyUp(GeneralKeyEventArgs keyEventArgs);
}