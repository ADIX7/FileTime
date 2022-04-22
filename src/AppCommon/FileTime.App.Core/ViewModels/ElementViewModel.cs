using FileTime.App.Core.Services;
using FileTime.Core.Models;
using MvvmGen;

namespace FileTime.App.Core.ViewModels
{
    [ViewModel(GenerateConstructor = false)]
    public partial class ElementViewModel : ItemViewModel, IElementViewModel
    {
        [Property]
        private long? _size;

        public ElementViewModel(IItemNameConverterService _itemNameConverterService, IAppState _appState) : base(_itemNameConverterService, _appState)
        {
        }

        public void Init(IElement item, ITabViewModel parentTab)
        {
            Init((IItem)item, parentTab);
        }
    }
}