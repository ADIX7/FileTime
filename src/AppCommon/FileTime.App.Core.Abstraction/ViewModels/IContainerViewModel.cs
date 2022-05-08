using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface IContainerViewModel : IItemViewModel, IInitable<IContainer, ITabViewModel, ItemViewModelType>
{
    IContainer? Container { get; }
}