using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels
{
    public interface IElementViewModel : IItemViewModel, IInitable<IElement, ITabViewModel>
    {
        long? Size { get; set; }
    }
}