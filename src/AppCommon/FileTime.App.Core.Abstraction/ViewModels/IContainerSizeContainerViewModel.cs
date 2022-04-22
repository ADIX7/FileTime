using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels
{
    public interface IContainerSizeContainerViewModel : IItemViewModel, IInitable<IContainer, ITabViewModel>
    {
        long Size { get; set; }
    }
}