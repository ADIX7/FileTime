using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels
{
    public interface IContainerViewModel : IItemViewModel, IInitable<IContainer, ITabViewModel, int>
    {
        IContainer? Container { get; }
    }
}