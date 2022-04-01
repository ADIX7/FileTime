
using FileTime.Core.Models;
using FileTime.Core.Services;
using InitableService;

namespace FileTime.App.Core.ViewModels
{
    public interface ITabViewModel : IInitable<ITab>
    {
        IObservable<IContainer?>? CurrentLocation { get; }
        IObservable<IItemViewModel?>? CurrentSelectedItem { get; }
        IObservable<IReadOnlyList<IItemViewModel>>? CurrentItems { get; }
        IObservable<IReadOnlyList<FullName>> MarkedItems { get; }
        ITab? Tab { get; }
    }
}