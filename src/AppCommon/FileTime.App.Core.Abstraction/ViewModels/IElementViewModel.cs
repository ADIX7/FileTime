using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;
using FileTime.Core.Traits;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface IElementViewModel :
    IItemViewModel,
    IInitable<IElement, ITabViewModel, ItemViewModelType>,
    ISizeProvider
{
    IElement? Element { get; }
}