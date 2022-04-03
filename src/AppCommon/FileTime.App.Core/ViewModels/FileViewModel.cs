using FileTime.App.Core.Services;
using FileTime.Core.Models;
using MvvmGen;

namespace FileTime.App.Core.ViewModels
{
    [ViewModel(GenerateConstructor = false)]
    public partial class FileViewModel : ElementViewModel, IFileViewModel
    {
        public FileViewModel(IItemNameConverterService _itemNameConverterService, IAppState _appState) : base(_itemNameConverterService, _appState)
        {
        }

        public void Init(IFileElement item, ITabViewModel parentTab, int index)
        {
            Init((IElement)item, parentTab, index);
        }
    }
}