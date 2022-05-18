using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using MvvmGen;

namespace FileTime.App.Core.ViewModels;

[ViewModel(GenerateConstructor = false)]
public partial class FileViewModel : ElementViewModel, IFileViewModel
{
    public FileViewModel(IItemNameConverterService itemNameConverterService, IAppState appState) : base(itemNameConverterService, appState)
    {
    }

    public void Init(IElement item, FileExtension fileExtension, ITabViewModel parentTab, ItemViewModelType itemViewModelType)
    {
        Init((IElement)item, parentTab, itemViewModelType);
        Size = fileExtension.Size;
    }
}