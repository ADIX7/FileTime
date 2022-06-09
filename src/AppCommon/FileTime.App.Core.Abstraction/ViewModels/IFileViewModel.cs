using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;
using FileTime.Core.Models.Extensions;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface IFileViewModel : IElementViewModel, IInitable<IElement, FileExtension, ITabViewModel, ItemViewModelType>
{
}