using FileTime.App.FrequencyNavigation.ViewModels;

namespace FileTime.App.FrequencyNavigation.Services;

public interface IFrequencyNavigationService
{
    IObservable<bool> ShowWindow { get; }
    IFrequencyNavigationViewModel? CurrentModal { get; }
    void OpenNavigationWindow();
    void CloseNavigationWindow();
    IList<string> GetMatchingContainers(string searchText);
}