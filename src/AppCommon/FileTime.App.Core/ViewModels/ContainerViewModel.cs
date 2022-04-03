using FileTime.App.Core.Services;
using FileTime.Core.Models;
using MvvmGen;

namespace FileTime.App.Core.ViewModels
{
    [ViewModel(GenerateConstructor = false)]
    public partial class ContainerViewModel : ItemViewModel, IContainerViewModel
    {
        public ContainerViewModel(IItemNameConverterService _itemNameConverterService, IAppState _appState) : base(_itemNameConverterService, _appState)
        {
        }

        public void Init(IContainer item, ITabViewModel parentTab, int index)
        {
            Init((IItem)item, parentTab, index);
        }
    }
}