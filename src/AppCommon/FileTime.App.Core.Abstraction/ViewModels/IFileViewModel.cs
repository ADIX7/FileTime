using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface IFileViewModel : IElementViewModel, IInitable<IElement, FileExtension, ITabViewModel, ItemViewModelType>
{
}