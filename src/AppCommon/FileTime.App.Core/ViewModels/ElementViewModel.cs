using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using MvvmGen;

namespace FileTime.App.Core.ViewModels;

[ViewModel(GenerateConstructor = false)]
public partial class ElementViewModel : ItemViewModel, IElementViewModel
{
    [Property]
    private long? _size;

    public ElementViewModel(IItemNameConverterService itemNameConverterService, IAppState appState) : base(itemNameConverterService, appState)
    {
    }

    public void Init(IElement item, ITabViewModel parentTab, ItemViewModelType itemViewModelType)
    {
        Init((IItem)item, parentTab, itemViewModelType);
    }
}