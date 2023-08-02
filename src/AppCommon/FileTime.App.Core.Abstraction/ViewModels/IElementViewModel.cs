using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Models.Traits;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface IElementViewModel :
    IItemViewModel,
    ISizeProvider,
    IInitable<IElement, ITabViewModel, ItemViewModelType>
{
    IElement? Element { get; }
}