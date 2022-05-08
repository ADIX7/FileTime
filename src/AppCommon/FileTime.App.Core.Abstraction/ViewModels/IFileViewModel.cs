using FileTime.App.Core.Models.Enums;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.ViewModels;

public interface IFileViewModel : IElementViewModel, IInitable<IFileElement, ITabViewModel, ItemViewModelType>
{
}