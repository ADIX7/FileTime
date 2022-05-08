using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface IContainerSizeContainerViewModel : IItemViewModel, IInitable<IContainer, ITabViewModel, ItemViewModelType>
{
    long Size { get; set; }
}