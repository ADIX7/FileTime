using DeclarativeProperty;
using FileTime.App.FrequencyNavigation.ViewModels;

namespace FileTime.App.FrequencyNavigation.Services;

public interface IFrequencyNavigationService
{
    IDeclarativeProperty<bool> ShowWindow { get; }
    IFrequencyNavigationViewModel? CurrentModal { get; }
    Task OpenNavigationWindow();
    void CloseNavigationWindow();
    ValueTask<IList<string>> GetMatchingContainers(string searchText);
}