using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels
{
    public interface IContainerSizeContainerViewModel : IItemViewModel, IInitable<IContainer, ITabViewModel, int>
    {
        long Size { get; set; }
    }
}