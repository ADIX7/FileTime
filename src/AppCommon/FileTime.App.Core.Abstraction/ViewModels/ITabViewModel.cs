
using FileTime.Core.Models;
using FileTime.Core.Services;
using InitableService;

namespace FileTime.App.Core.ViewModels
{
    public interface ITabViewModel : IInitable<ITab, int>
    {
        ITab? Tab { get; }
        int TabNumber { get; }
        IObservable<bool> IsSelected { get; }
        IObservable<IContainer?> CurrentLocation { get; }
        IObservable<IItemViewModel?> CurrentSelectedItem { get; }
        IObservable<IReadOnlyList<IItemViewModel>> CurrentItems { get; }
        IObservable<IEnumerable<FullName>> MarkedItems { get; }
        IObservable<IReadOnlyList<IItemViewModel>?> SelectedsChildren { get; }
    }
}