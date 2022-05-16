using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface IElementViewModel : IItemViewModel, IInitable<IElement, ITabViewModel, ItemViewModelType>
{
    IElement? Element { get; }
    long? Size { get; set; }
}